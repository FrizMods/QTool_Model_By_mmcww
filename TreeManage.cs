using  System ;
using  System . Collections . Generic ;
using  System . Threading ;

namespace  Qtools
{
    public  class  TreeManage
    {

        public  enum  RunTask
        {
            TaskNone ,
            TaskSort ,
            TaskStatistc
        };
        public  Thread  calThread ;
        public  AutoResetEvent  sortEvent = new  AutoResetEvent ( false );
        public  Thread  sortThread ;
        public  AutoResetEvent  calEvent  =  new  AutoResetEvent ( false );


        public  List < RootItem > rootItems  =  new  List < RootItem >();
        private  HashSet < int > outSet  =  new  HashSet < int >();
        private  HashSet < int > inSet  =  new  HashSet < int >();

        public  TreeManage ()
        {
            InitThread ();
        }

        private  void  InitThread ()
        {
            sortThread  =  new  Thread ( new  ThreadStart ( RunSort ));
            sortThread . IsBackground  =  true ;
            calThread  =  new  Thread ( new  ThreadStart ( RunCal ));
            calThread . IsBackground  =  true ;
            sortThread . Start ();
            calThread . Start ();
        }


        // ==============================================Calculation Result statistics
        public  Dictionary < int , float > ansNeedInput  =  new  Dictionary < int , float >();
        /// < summary >
        /// [id] = quantity
        /// </ summary >
        public  Dictionary < int , int > ansFactory  =  new  Dictionary < int , int >();
        /// < summary >
        /// [0] Maximum power consumption [1] Rated power consumption [2] Standby power consumption
        /// </ summary >
        public  long [] ansPower  =  new  long [ 3 ] { 0 , 0 , 0 };
        /// < summary >
        /// Statistics of the number of claws, three sorters id 2011 2012 2013
        /// </ summary >
        public  int  ansPicker  =  0 ;

        public  int  ansOutCount  =  0 ;

        /// < summary >
        /// Add product
        /// [Features] Will update the output table and input table to update all trees
        /// </ summary >
        /// < param  name = " top " ></ param >
        public  void  AddTopItem ( RootItem  rootNode )
        {
            rootNode . SetInAndOut ( inSet , outSet ). UpdateTree ( 0 );
            ansOutCount  +=  rootNode . outItems . Count ;
            foreach ( int  itemId  in  rootNode . ansProductTimes . Keys ) {
                if ( rootNode . ansProductTimes [ itemId ] <  0 ) {
                    if ( ! inSet . Contains ( itemId )) {
                        inSet . Add ( itemId );
                    }
                }
                if ( rootNode . ansProductTimes [ itemId ] >  0 ) {
                    if ( ! outSet . Contains ( itemId )) {
                        outSet . Add ( itemId );
                    }
                }
            }
            foreach ( RootItem  rootItem  in  rootItems ) {
                rootItem . SetInAndOut ( inSet , outSet ). UpdateTree ( 0 );
            }
            // Update the root node table
            rootItems . Add ( rootNode );
            // //Update the output table to consider multiple products
            // Factory factory = (Factory)rootNode.inputNodes[0];
            // //Delete sub-nodes of the same recipe in other trees
            // foreach (TreeNode topNode in rootItems) {
            //     DeleteDestinationRecipe(topNode, factory.theRecipe);
            // }
        }
        /// < summary >
        /// Delete root product delete raw material list
        /// </ summary >
        /// < param  name = " top " ></ param >
        public  void  DeleteTopItem ( TreeNode  top )
        {
            if ( top . layer  ==  0 ) {
                RootItem  topRoad  = ( RootItem ) top . MyRootNode ;
                ansOutCount  -=  topRoad . outItems . Count ;
                rootItems . Remove ( topRoad );
                foreach ( int  outItemId  in  topRoad . ansProductTimes . Keys ) {
                    if ( topRoad . ansProductTimes [ outItemId ] >  0  &&  ! IsOutHave ( outItemId )) {
                        outSet . Remove ( outItemId );
                    }
                    if ( topRoad . ansProductTimes [ outItemId ] <  0  &&  ! IsInHave ( outItemId )) {
                        inSet . Remove ( outItemId );
                    }
                }
            }
        }

        public  void  ClearTopItem ()
        {
            calThread . Abort ();
            sortThread . Abort ();
            rootItems . Clear ();
            outSet . Clear ();
            ansNeedInput . Clear ();
            ansFactory . Clear ();
            ansNeedInput . Clear ();
            ansPicker  =  0 ;
            ansPower [ 0 ] =  0 ;
            ansPower [ 1 ] =  0 ;
            ansPower [ 2 ] =  0 ;
            ansOutCount  =  0 ;
            InitThread ();
        }




        /// < summary >
        /// Set the recipe as a module
        /// [Features] Simultaneously initialize the module parameter list
        /// [Restrictions] Leaf nodes cannot be set as modules
        /// </ summary >
        public  void  ToModule ( TreeNode  theNode )
        {
            ItemRoad  road ;
            RecipeProto  recipe ;
            // No matter if the current node is road or factory, get its recipe
            if ( theNode  is  Factory ) {
                road  =  theNode . outputNodes [ 0 ] as  ItemRoad ;
                recipe  = (( Factory ) theNode ). theRecipe ;
            }
            else  if ( theNode . inputNodes . Count  >  0 ) {
                road  = ( ItemRoad ) theNode ;
                recipe  = ( theNode . inputNodes [ 0 ] as  Factory ). theRecipe ;
            }
            else {
                return ;
            }
            //The final product (may be a by-product)
            if ( road . layer  ==  0 ) {
                return ;
            }
            // Root node add
            RootItem  moduleRoad  =  new  RootItem ( road );
            AddTopItem ( moduleRoad );
            //The current tree structure has changed
            //The tree structure changes height and width...
            // ((RootItem)theNode.myRootNode).UpdateTree();
        }

        /// < summary >
        /// Switch recipe
        /// < para >【Restrictions】The root node cannot be switched to self-provided by-products and cannot be modified.</ para >
        /// </ summary >
        /// < returns >true valid switch/false invalid switch</ returns >
        public  TreeNode  ChangeRecipe ( TreeNode  node )
        {
            ItemRoad  theNode  =  node  as  ItemRoad ;
            TreeNode  ansNode ;
            if ( theNode  !=  null ) {
                // Modular products cannot switch recipes
                // if (outSet.Contains(theNode.TheItem.ID) && theNode.layer!=0) {
                //     return false;
                // }
                // ========= The root node does not allow the formula to be none
                if ( theNode . outputNodes . Count  ==  0 ) {
                    //By -products cannot be modified
                    if ( theNode . layer  !=  0 ) {
                        return  theNode ;
                    }
                    if ( theNode . TheItem . recipes . Count  >  1 ) {
                        // Root node switching recipe and other same multi-product root node switching recipes may change the structure of all trees
                        theNode . recipeIndex ++ ;
                        if ( theNode . recipeIndex  ==  theNode . TheItem . recipes . Count ) {
                            theNode . recipeIndex  =  0 ;
                        }
                        DeleteTopItem ( theNode . MyRootNode );
                        ansNode  =  new  RootItem ( theNode );
                        AddTopItem (( RootItem ) ansNode );
                        // Inherit the original user settings
                        (( RootItem ) ansNode ). outItems [( ItemRoad ) ansNode ][ 0 ] = (( RootItem ) theNode . MyRootNode ). outItems [ theNode ][ 0 ];
                        (( RootItem ) ansNode ). outItems [( ItemRoad ) ansNode ][ 1 ] = (( RootItem ) theNode . MyRootNode ). outItems [ theNode ][ 1 ];
                        return  ansNode ;
                    }
                    else {
                        return  theNode ;
                    }
                }
                // ======== other nodes
                else {
                    theNode . recipeIndex ++ ;
                    if ( theNode . recipeIndex  ==  theNode . TheItem . recipes . Count ) {
                        theNode . recipeIndex  =  - . 1 ;
                    }
                    //The tree structure changes height and width...
                    (( RootItem ) theNode . MyRootNode ). UpdateTree ( 0 );
                    return  theNode ;
                }
            }
            return  theNode ;
        }


        



        private  void  RunSort ()
        {
            while ( true ) {
                sortEvent . WaitOne ();
                rootItems  =  SortLevel ( true );
            }
        }
        private  void  RunCal ()
        {
            while ( true ) {
                calEvent . WaitOne ();
                StatisticsDownResult ();
            }
        }


        /// < summary >
        /// Sort the product tree according to the output level from high to low
        /// [Features] Multi-products only add the kind that exists in rootNodes
        /// </ summary >
        public  List < RootItem > SortLevel ( bool  isLowToHigh  =  false )
        {
            // Check the auxiliary matrix
            Dictionary < RootItem , bool > checkArray  =  new  Dictionary < RootItem , bool >();
            List < TreeNode > multiTop  =  new  List < TreeNode >();
            // Auxiliary matrix initialization
            foreach ( RootItem  topNode  in  rootItems ) {
                checkArray [ topNode ] =  false ;
            }
            // Result list
            List < RootItem > theLevel  =  new  List < RootItem >();
            // Select sort
            int  ix  =  0 ;
            while ( theLevel . Count  <  rootItems . Count ) {
                // QTools.QTool.DebugLog($"ix:{ix++}/cout:{theLevel.Count}");
                // For a root node
                foreach ( RootItem  topNode  in  rootItems ) {
                    // (Filter the root node that has been sorted)
                    if ( checkArray [ topNode ])
                        continue ;
                    bool  isHave  =  false ;
                    // All products
                    foreach ( int  productId  in  topNode . ansProductTimes . Keys ) {
                        // This is a demand material
                        if ( topNode . ansProductTimes [ productId ] <=  0 ) {
                            continue ;
                        }

                        // === The remaining root node is traversed
                        foreach ( RootItem  topNode1  in  rootItems ) {
                            // (Filter the sorted root node and yourself)
                            if ( checkArray [ topNode1 ] ||  topNode1  ==  topNode )
                                continue ;
                            foreach ( int  productId1  in  topNode1 . ansProductTimes . Keys ) {
                                // This is a supply material
                                if ( topNode . ansProductTimes [ productId ] >=  0 ) {
                                    continue ;
                                }
                                // If it is not in the remaining root node raw material list
                                if ( productId  ==  productId1 ) {
                                    isHave  =  true ;
                                    break ;
                                }
                            }
                            if ( isHave )
                                break ;
                        }
                        if ( isHave )
                            break ;
                    }
                    // The product level of the root node is higher than the product level of the remaining root nodes
                    if ( ! isHave ) {
                        checkArray [ topNode ] =  true ;
                        theLevel . Add ( topNode );
                    }
                }
            }
            if ( isLowToHigh ) {
                theLevel . Reverse ();
                rootItems  =  theLevel ;
            }
            return  theLevel ;
        }
        /// < summary >
        /// Check whether there is a certain item in the output products of all trees
        /// [Performance] Worst case O(n)
        /// </ summary >
        /// < param  name = " itemId " ></ param >
        /// < returns ></ returns >
        public  bool  IsOutHave ( int  itemId )
        {
            foreach ( RootItem  topNode  in  rootItems ) {
                foreach ( ItemRoad  outProduct  in  topNode . outItems . Keys ) {
                    if ( outProduct . TheItem . ID  ==  itemId ) {
                        return  true ;
                    }
                }
            }
            return  false ;
        }
        /// < summary >
        /// Check whether there is a certain item in the input products of all trees
        /// [Performance] Worst case O(n)
        /// </ summary >
        /// < param  name = " itemId " ></ param >
        /// < returns ></ returns >
        public  bool  IsInHave ( int  itemId )
        {
            foreach ( RootItem  topNode  in  rootItems ) {
                foreach ( ItemRoad  inProduct  in  topNode . inItems ) {
                    if ( inProduct . TheItem . ID  ==  itemId ) {
                        return  true ;
                    }
                }
            }
            return  false ;
        }

        /// < summary >
        /// Get the tree that produces a specific product
        /// </ summary >
        /// < param  name = " itemId " ></ param >
        /// < returns ></ returns >
        public  List < RootItem > GetTopItemNodes ( int  itemId )
        {
            List < RootItem > ans  =  new  List < RootItem >();
            foreach ( RootItem  topNode  in  rootItems ) {
                foreach ( ItemRoad  outProduct  in  topNode . outItems . Keys ) {
                    if ( outProduct . TheItem . ID  ==  itemId ) {
                        ans . Add ( topNode );
                        break ;
                    }
                }
            }
            return  ans ;
        }
        



        /// < summary >
        /// Cannot delete the recipe tree of the root node
        /// </ summary >
        /// < param  name = " theNode " ></ param >
        /// < param  name = " recipeProto " ></ param >
        private  static  void  DeleteDestinationRecipe ( TreeNode  theNode , RecipeProto  recipeProto )
        {
            foreach ( TreeNode  node  in  theNode . inputNodes ) {
                if ( node . GetType (). Name  ==  " ItemRoad "  &&  node . inputNodes . Count  >  0 ) {
                    if (( node . inputNodes [ 0 ] as  Factory ). theRecipe  ==  recipeProto ) {
                        node . inputNodes . Clear ();
                        continue ;
                    }
                }
                DeleteDestinationRecipe ( node , recipeProto );
            }
        }


        /// < summary >
        /// Statistics of raw materials needed every minute
        /// </ summary >
        /// < returns ></ returns >
        public  void  StatisticsInputResult ()
        {
            ansNeedInput . Clear ();
            foreach ( RootItem  topRoad  in  rootItems ) {
                foreach ( ItemRoad  itemInput  in  topRoad . inItems ) {
                    if ( ! IsOutHave ( itemInput . TheItem . ID )) {
                        if ( ! ansNeedInput . ContainsKey ( itemInput . TheItem . ID )) {
                            ansNeedInput [ itemInput . TheItem . ID ] =  itemInput . needNumPerMin ;
                        }
                        else {
                            ansNeedInput [ itemInput . TheItem . ID ] +=  itemInput . needNumPerMin ;
                        } 
                    }
                }
            }
        }
        public  void  StatisticsDownResult ()
        {
            // ================== Initialize the statistics table
            ansFactory . Clear ();
            ansNeedInput . Clear ();
            ansPicker  =  0 ;
            ansPower [ 0 ] =  0 ;
            ansPower [ 1 ] =  0 ;
            ansPower [ 2 ] =  0 ;
            // Initialize the scale
            // CalAllOutRatio();
            HashSet < int > calRatio  =  new  HashSet < int >();

            List < RootItem > levelResult  =  SortLevel ();
            // Traverse all products
            for ( int  i  =  0 ; i  <  levelResult . Count ; i ++ ) {
                RootItem  topRoad  =  levelResult [ i ];
                foreach ( ItemRoad  topOut  in  topRoad . outItems . Keys ) {
                    topOut . needNumPerMin  =  0 ;
                    // Calculate the demand as a module
                    // Traverse the corresponding raw materials
                    bool  isNeedTheItem  =  false ;
                    for ( int  j  =  0 ; j  <  i ; j ++ ) {
                        RootItem  topRoad2  =  levelResult [ j ];
                        foreach ( ItemRoad  topOut2  in  topRoad2 . inItems ) {
                            if ( topOut2 . TheItem . ID  ==  topOut . TheItem . ID ) {
                                topOut . needNumPerMin  +=  topOut2 . needNumPerMin ;
                                isNeedTheItem  =  true ;
                            }
                        }
                    }
                    // Calculate the ratio
                    if ( ! calRatio . Contains ( topOut . TheItem . ID ) &&  isNeedTheItem ) {
                        calRatio . Add ( topOut . TheItem . ID );
                        float  sum  =  0 ;
                        int  sum2  =  0 ;
                        // Sum
                        for ( int  j  =  i ; j  <  levelResult . Count ; j ++ ) {
                            RootItem  topRoad2  =  levelResult [ j ];
                            foreach ( ItemRoad  topOut2  in  topRoad2 . outItems . Keys ) {
                                if ( topOut2 . TheItem . ID  ==  topOut . TheItem . ID ) {
                                    sum  +=  topRoad2 . outItems [ topOut2 ][ 1 ];
                                }
                            }
                        }
                        // Normalize
                        for ( int  j  =  i ; j  <  levelResult . Count ; j ++ ) {
                            RootItem  topRoad2  =  levelResult [ j ];
                            foreach ( ItemRoad  topOut2  in  topRoad2 . outItems . Keys ) {
                                if ( topOut2 . TheItem . ID  ==  topOut . TheItem . ID ) {
                                    // Round down, sum2 can only be less than 100
                                    topRoad2 . outItems [ topOut2 ][ 1 ] = ( int )( topRoad2 . outItems [ topOut2 ][ 1 ] /  sum  *  100 );
                                    sum2  +=  topRoad2 . outItems [ topOut2 ][ 1 ];
                                }
                            }
                        }
                        // Optimization error
                        for ( int  j  =  i ; j  <  levelResult . Count ; j ++ ) {
                            RootItem  topRoad2  =  levelResult [ j ];
                            foreach ( ItemRoad  topOut2  in  topRoad2 . outItems . Keys ) {
                                if ( topOut2 . TheItem . ID  ==  topOut . TheItem . ID ) {
                                    // Round down, sum2 can only be less than 100
                                    topRoad2 . outItems [ topOut2 ][ 1 ] +=  100  -  sum2 ;
                                    j  =  levelResult . Count  +  1 ;
                                    break ;
                                }
                            }
                        }
                    }
                    
                    topOut . needNumPerMin  *=  topRoad . outItems [ topOut ][ 1 ] /  100 f ;
                    // Write user requirements
                    topOut . needNumPerMin  +=  topRoad . outItems [ topOut ][ 0 ];
                }
                // Calculate input raw material requirements
                topRoad . StatisticsDownResult ();
                // Statistical input
                foreach ( ItemRoad  outputRoad  in  topRoad . outItems . Keys ) {
                    if ( ! ansNeedInput . ContainsKey ( outputRoad . TheItem . ID )) {
                        ansNeedInput [ outputRoad . theItem . ID ] =  - outputRoad . needNumPerMin ;
                    }
                    else {
                        ansNeedInput [ outputRoad . TheItem . ID ] -=  outputRoad . needNumPerMin ;
                    }
                }

                foreach ( ItemRoad  inputRoad  in  topRoad . inItems ) {
                    if ( ! ansNeedInput . ContainsKey ( inputRoad . TheItem . ID )) {
                        ansNeedInput [ inputRoad . TheItem . ID ] =  inputRoad . needNumPerMin ;
                    }
                    else {
                        ansNeedInput [ inputRoad . TheItem . ID ] +=  inputRoad . needNumPerMin ;
                    }
                }
            }
            // =================================== Statistics input raw material table
            // StatisticsInputResult();
            // ============== Statistics cost
            foreach ( RootItem  rootItem  in  rootItems ) {
                foreach ( int  factorId  in  rootItem . ansFactory . Keys ) {
                    if ( ! ansFactory . ContainsKey ( factorId )) {
                        ansFactory [ factorId ] =  rootItem . ansFactory [ factorId ];
                    }
                    else {
                        ansFactory [ factorId ] +=  rootItem . ansFactory [ factorId ];
                    }
                }
                ansPicker  +=  rootItem . ansPicker ;
                ansPower [ 0 ] +=  rootItem . ansPower [ 0 ];
                ansPower [ 1 ] +=  rootItem . ansPower [ 1 ];
                ansPower [ 2 ] +=  rootItem . ansPower [ 2 ];
            }
        }
        public  void  CalOutRatio ( int  itemId )
        {
            if ( outSet . Contains ( itemId )) {
                float  sum  =  0 ;
                int  sum2  =  0 ;
                // Sum
                foreach ( RootItem  rootOut  in  rootItems ) {
                    foreach ( ItemRoad  itemRoad  in  rootOut . outItems . Keys ) {
                        if ( itemRoad . TheItem . ID  ==  itemId ) {
                            sum  +=  rootOut . outItems [ itemRoad ][ 1 ];
                        }
                    }
                }
                // Normalize
                foreach ( RootItem  rootOut  in  rootItems ) {
                    foreach ( ItemRoad  itemRoad  in  rootOut . outItems . Keys ) {
                        if ( itemRoad . TheItem . ID  ==  itemId ) {
                            // Round down, sum2 can only be less than 100
                            rootOut . outItems [ itemRoad ][ 1 ] = ( int )( rootOut . outItems [ itemRoad ][ 1 ] /  sum  *  100 );
                            sum2  +=  rootOut . outItems [ itemRoad ][ 1 ];
                        }
                    }
                }
                // Optimization error
                foreach ( RootItem  rootOut  in  rootItems ) {
                    foreach ( ItemRoad  itemRoad  in  rootOut . outItems . Keys ) {
                        if ( itemRoad . TheItem . ID  ==  itemId ) {
                            // Round down, sum2 can only be less than 100
                            rootOut . outItems [ itemRoad ][ 1 ] +=  100  -  sum2 ;
                            return ;
                        }
                    }
                }

            }
        }
        public  void  CalAllOutRatio ()
        {
            foreach ( int  id  in  outSet ) {
                CalOutRatio ( id );
            }
        }
        public  List < ItemRoad > GetAllOut ()
        {
            List < ItemRoad > ansItems  =  new  List < ItemRoad >();
            ForEachOut (( x ) => {
                ansItems . Add ( x );
            });
            return  ansItems ;
        }

        public  int  ForEachOut ( Action < ItemRoad > action )
        {
            int  cnt  =  0 ;
            foreach ( RootItem  rootItem  in  rootItems ) {
                foreach ( ItemRoad  itemRoad  in  rootItem . outItems . Keys ) {
                    action ( itemRoad );
                    cnt ++ ;
                }
            }
            return  cnt ;
        }
    }
}