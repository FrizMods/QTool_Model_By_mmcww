using  Qtools ;
using  QTools ;
using  System ;
using  System . Collections . Generic ;
using  System . Linq ;
using  System . Text ;
using  UnityEngine ;

namespace  Qtools
{
    /*
     * Determine the height and width of the tree
     * Generate auxiliary array when drawing graphics, record the maximum coordinates of the rightmost end of each layer
     * 
     */
    /// < summary >
    /// < para >The node base class of the graph, serving the UI</ para >
    /// </ summary >
    public  class  TreeNode : IComparable < TreeNode >
    {
        /// < summary >
        /// UI location
        /// </ summary >
        public  Rect  posRect  =  new  Rect ();
        public  static  GUIStyle  uiButtonStyle ;
        public  static  GUIStyle  uiLabelStyle ;
        public  static  bool  isNeedToModule  =  false ;



        // =============================================== logic


        protected  static  Action < TreeNode > Statistics ;

        /// < summary >
        /// The height leaf node of the logical location tree is 1 and it is valid when the tree is built from the bottom up
        /// </ summary >
        public  int  treeHight  =  1 ;
        /// < summary >
        /// The width of the logical location tree is 1 and the leaf node is valid when the tree is built from the bottom up
        /// </ summary >
        public  int  treeWidth  =  1 ;
        /// < summary >
        ///The root node of the logical layer label of the tree is 0, which is valid when the tree is built from top to bottom
        /// </ summary >
        public  int  layer  =  0 ;

        public  TreeNode  myRootNode ;

        /// < summary >
        /// itemId->conveyor belt->quantity/second
        /// Enter only one path for each item
        /// </ summary >
        public  List < TreeNode > inputNodes  =  new  List < TreeNode >();
        /// < summary >
        /// itemId->conveyor belt->quantity/second
        ///The output may have multiple outputs of the same type
        /// </ summary >
        public  List < TreeNode > outputNodes  =  new  List < TreeNode >();

        public  TreeNode ()
        {
        }
        public  TreeNode ( TreeNode  node ) {

        }

        /// < summary >
        /// UI drawing
        /// </ summary >
        /// < returns ></ returns >
        public  virtual  bool  UIDraw ()
        {
            return  false ;
        }
        public  virtual  int  GetItemId ()
        {
            return  0 ;
        }

        public  virtual  void  UpdateTree ( int  parentItemId )
        { 
        
        }
        
        public  virtual  void  StatisticsDownResult ()
        {
        
        }
        public  int  CompareTo ( TreeNode  other )
        {
            if ( this . treeHight  >  other . treeHight ) {
                return  1 ;
            }
            else  if ( this . treeHight  <  other . treeHight ) {
                return  - 1 ;
            }
            else
                return  0 ;
        }


        


        
    }
    /// < summary >
    /// Factory output and input strictly correspond to the formula
    /// </ summary >
    public  class  Factory : TreeNode
    {
        /// < summary >
        /// Factory type id
        /// </ summary >
        private  int  factoryId ;
        public  RecipeProto  theRecipe ;
        /// < summary >
        /// Factory efficiency
        /// </ summary >
        private  float  efficiency  =  1 f ;
        private  float  speed  =  1 f ;
        /// < summary >
        /// Number of factories
        /// </ summary >
        private  int  num ;

        public  Dictionary < int , TreeNode > inputFindChart ;
        public  Dictionary < int , TreeNode > outputFindChart ;

        public  float  Efficiency { get  =>  efficiency ; private  set  =>  efficiency  =  value ;}
        public  int  FactoryId { get  =>  factoryId ; private  set  =>  factoryId  =  value ;}
        public  int  Num { get  =>  num ; set  =>  num  =  value ;}

        /// < summary >
        /// Automatically added to allNodes, speed defaults to 1
        /// </ summary >
        /// < param  name = " theRecipe " ></ param >
        public  Factory ( RecipeProto  theRecipe ): base ()
        {
            this . theRecipe  =  theRecipe ;
            inputFindChart  =  new  Dictionary < int , TreeNode >();
            outputFindChart  =  new  Dictionary < int , TreeNode >();
            GetFactoryId ( 1 f );
        }
        /// < summary >
        /// Get factory id
        /// </ summary >
        /// < param  name = " speed " ></ param >
        /// < returns ></ returns >
        private  int  GetFactoryId ( float  speed )
        {
            switch ( theRecipe . Type ) {
                case  ERecipeType . None :
                    break ;
                case  ERecipeType . Smelt :
                    factoryId  =  2302 ;
                    break ;
                case  ERecipeType . Chemical :
                    factoryId  =  2309 ;
                    break ;
                case  ERecipeType . Refine :
                    factoryId  =  2308 ;
                    break ;
                case  ERecipeType . Assemble :
                    // Three manufacturing stations
                    IF ( Speed  ==  0 . 75 F ) {
                        factoryId  =  2303 ;
                    }
                    else  if ( speed  ==  1 f ) {
                        factoryId  =  2304 ;
                    }
                    the else  IF ( Speed  ==  . 1 . . 5 F ) {
                        factoryId  =  2305 ;
                    }
                    break ;
                case  ERecipeType . Particle :
                    factoryId  =  2310 ;
                    break ;
                case  ERecipeType . Exchange :
                    factoryId  =  2209 ;
                    break ;
                case  ERecipeType . PhotonStore :
                    factoryId  =  2208 ;
                    break ;
                case  ERecipeType . Fractionate :
                    factoryId  =  2314 ;
                    break ;
                case  ERecipeType . Research :
                    factoryId  =  2901 ;
                    break ;
            }
            return  factoryId ;
        }
        public  override  bool  UIDraw ()
        {
            bool  isClick  =  false ;
            if ( GUI . Button ( posRect , LDB . items . Select ( factoryId ). iconSprite . texture , uiButtonStyle )) {
                isClick  =  true ;
                // Making station
                if ( factoryId  >=  2303  &&  factoryId  <=  2305 ) {
                    if ( Input . GetKey ( KeyCode . LeftControl )) {
                        ChangFactory ();
                    }
                }
                if ( Input . GetKey ( KeyCode . LeftAlt )) {
                    isNeedToModule  =  true ;
                }

                // isUpdate = true;
            }
            The GUI . The Label ( new new  Rect ( posRect . X , posRect . Y  +  posRect . Height , posRect . Width , posRect . Height ), $ " { LDB . Items . The Select ( factoryId ). Name } \ n- { QTool . Translate ( " Quantity " )}: { num } " ,uiLabelStyle );
            return  isClick ;
        }
        public  override  int  GetItemId ()
        {
            return  factoryId ;
        }
        public  void  ChangFactory ()
        {
            if ( factoryId  >=  2303  &&  factoryId  <=  2305 ) {
                factoryId  = ( factoryId  -  2303  +  1 ) %  3  +  2303 ;
            }
        }


        /// < summary >
        /// [Characteristics] As long as the recipe of the factory remains unchanged, its child node will not change if the parent node changes or if the parent node recipe changes, this node is a new node
        ///A new tree will be generated
        /// </ summary >
        /// < param  name = " offerInputRoads " >Raw materials already provided</ param >
        public  override  void  UpdateTree ( int  parentItemId )
        {
            treeWidth  =  0 ;
            treeHight  =  0 ;
            float  times  =  0 ;
            for ( int  i  =  0 ; i  <  theRecipe . Results . Length ; i ++ ) {
                if ( theRecipe . Results [ i ] ==  parentItemId ) {
                    times  = (( ItemRoad ) outputFindChart [ parentItemId ]). productTimes  /  theRecipe . ResultCounts [ i ];
                }
            }
            if ( theRecipe . Type  ==  ERecipeType . Fractionate ) {
                times  /=  100 f ;
            }

            // Traverse raw materials
            for ( int  i  =  0 ; i  <  theRecipe . Items . Length ; i ++ ) {
                int  itemId  =  theRecipe . Items [ i ];
                if ( ! inputFindChart . ContainsKey ( itemId )) {
                    inputFindChart [ itemId ] =  new  ItemRoad ( LDB . items . Select ( itemId ));
                    inputFindChart [ itemId ]. outputNodes . Add ( this );
                    inputFindChart [ itemId ]. layer  =  layer  +  1 ;
                    inputFindChart [ itemId ]. myRootNode  =  myRootNode ;
                    // Initialize the magnification
                    (( ItemRoad ) inputFindChart [ itemId ]). ProductTimes  =  - Times  *  theRecipe . ItemCounts [ I ];
                }
                ( inputFindChart [ itemId ] as  ItemRoad ). UpdateTree ( factoryId );
                treeWidth  +=  inputFindChart [ itemId ]. treeWidth ;
                treeHight  =  treeHight  >  inputFindChart [ itemId ]. treeHight  ?  treeHight  :  inputFindChart [ itemId ]. treeHight ;
            }
            treeHight  +=  1 ;
            // Traversal product
            for ( int  i  =  0 ; i  <  theRecipe . Results . Length ; i ++ ) {
                int  itemId  =  theRecipe . Results [ i ];
                if ( ! outputFindChart . ContainsKey ( itemId )) {
                    outputFindChart [ itemId ] =  new  ItemRoad ( LDB . items . Select ( itemId ));
                    outputFindChart [ itemId ]. inputNodes . Add ( this );
                    outputFindChart [ itemId ]. myRootNode  =  myRootNode ;
                    for ( int  ii  =  0 ; ii  <  LDB . items . Select ( itemId ). recipes . Count ; ii ++ ){
                        if ( LDB . items . Select ( itemId ). recipes [ ii ] ==  theRecipe ) {
                            (( ItemRoad ) outputFindChart [ itemId ]). recipeIndex  =  ii ;
                            break ;
                        }
                    }
                    outputFindChart [ itemId ]. layer  =  layer  -  1 ;
                    outputFindChart [ itemId ]. treeWidth  =  treeWidth ;
                    outputFindChart [ itemId ]. treeHight  =  treeHight  +  1 ;
                    // Initialize the magnification
                    (( ItemRoad ) outputFindChart [ itemId ]). productTimes  =  times  *  theRecipe . ResultCounts [ i ];
                }
                if ( outputFindChart [ itemId ]. outputNodes . Count  ==  0 ) {
                    (( ItemRoad ) outputFindChart [ itemId ]). UpdateMultiItem ();
                }
            }
            outputNodes  =  outputFindChart . Values . ToList ();
            inputNodes  =  inputFindChart . Values . ToList ();
            // =============================== sort
            // inputNodes.Sort();
        }

        public  override  void  StatisticsDownResult ()
        {
            double  times  =  0 ;
            // Calculate formula magnification
            for ( int  i  =  0 ; i  <  theRecipe . Results . Length ; i ++ ) {
                double  ttimes  = ( outputFindChart [ theRecipe . Results [ i ]] as  ItemRoad ). needNumPerMin  /  theRecipe . ResultCounts [ i ];
                // Optimize by overcapacity
                times  =  times  >  ttimes  ?  times  :  ttimes ;
            }
            double  factCount  =  0 ;
            // ===========================Special distillation device
            if ( theRecipe . Type  !=  ERecipeType . Fractionate ) {
                factCount  =  Times  /  Speed  /  60  /  60  *  theRecipe . TimeSpend ;

            }
            else {
                float  productPer  =  1800 f  /  100 f ;
                factCount  = (( ItemRoad ) outputFindChart [ theRecipe . Results [ 0 ]]). needNumPerMin  /  productPer ;
            }

            if (( int ) factCount  <  factCount ) {
                num  = ( int ) factCount  +  1 ;
                Efficiency  = ( float )( factCount  /  num );
            }
            else {
                num  = ( int ) factCount ;
                Efficiency  =  1 f ;
            }
            Statistics ? . The Invoke ( the this );

            // Calculate the actual output
            for ( int  i  =  0 ; i  <  theRecipe . Results . Length ; i ++ ) {
                ( outputFindChart [ theRecipe . Results [ i ]] as  ItemRoad ). factOfferPerMin  = ( float )( times  *  theRecipe . ResultCounts [ i ]);
            }
            // Calculate input requirements
            // ===========================Special distillation device
            if ( theRecipe . Type  !=  ERecipeType . Fractionate ) {
                for ( int  i  =  0 ; i  <  theRecipe . Items . Length ; i ++ ) {
                    ( inputFindChart [ theRecipe . Items [ i ]] as  ItemRoad ). needNumPerMin  = ( float )( times  *  theRecipe . ItemCounts [ i ]);
                    inputFindChart [ theRecipe . Items [ i ]]. StatisticsDownResult ();
                }
            }
            else {
                (( ItemRoad ) inputFindChart [ theRecipe . Items [ 0 ]]). needNumPerMin  = (( ItemRoad ) outputFindChart [ theRecipe . Results [ 0 ]]). needNumPerMin ;
                inputFindChart [ theRecipe . Items [ 0 ]]. StatisticsDownResult ();
            }
        }
    }
    /// < summary >
    ///Conveyor belt model, single input, arbitrary output
    /// </ summary >
    public  class  ItemRoad : TreeNode
    {
        /// < summary >
        /// Production rate, specify> 0 for output < 0 for demand
        /// </summary>
        public  float  product  =  0 ;
        /// < summary >
        ///Multiplier of the production rate (the production volume when the top output is 60 pcs/min), the stipulation >0 is the output < 0 is the demand
        /// </summary>
        public  float  productTimes  =  0 ;
        /// < summary >
        /// Goods on the conveyor belt
        /// </ summary >
        protected  ItemProto  theItem ;
        /// < summary >
        /// Delivery volume per minute
        /// </ summary >
        public  float  needNumPerMin  =  0 ;
        public  float  factOfferPerMin  =  0 ;
        /// < summary >
        /// Recipe index (-1 self-provided)
        /// </ summary >
        public  int  recipeIndex  =  0 ;

        protected  static  Action < ItemRoad > LeafUpdateCallback ;
        protected  static  Action < ItemRoad > MultiUpdateCallback ;

        public  ItemProto  TheItem { get  =>  theItem ; private  set  =>  theItem  =  value ;}
        public  int  RecipeIndex { get  =>  recipeIndex ; private  set  =>  recipeIndex  =  value ;}

        public  ItemRoad ( int  itemId ): base ()
        {
            theItem  =  LDB . items . Select ( itemId );
        }
        public  ItemRoad ( ItemProto  theItem ): base ()
        {
            this . TheItem  =  theItem ;
        }
        public  ItemRoad ( ItemProto  theItem , int  recipeIndex ): base ()
        {
            this . TheItem  =  theItem ;
            this . recipeIndex  =  recipeIndex  >  theItem . recipes . Count  ?  0  :  recipeIndex ;
        }
        public  ItemRoad ( ItemRoad  road ): base ( road )
        {
            theItem  =  road . theItem ;
            recipeIndex  =  road . recipeIndex ;
        }

        public  override  bool  UIDraw ()
        {
            bool  isClick  =  false ;
            if ( TheItem . recipes . Count  >  0 ) {
                if ( GUI . Button ( posRect , TheItem . iconSprite . texture , uiButtonStyle )) {
                    isClick  =  true ;
                    if ( Input . GetKey ( KeyCode . LeftControl )) {
                        isNeedChangeRecipe  =  true ;
                    }
                    else  if ( Input . GetKey ( KeyCode . LeftAlt )) {
                        isNeedToModule  =  true ;
                    }
                }
            }
            else {
                GUI . Box ( posRect , TheItem . IconSprite . Texture );
            }
            GUI . Label ( new  Rect ( posRect . X , posRect . Y  +  posRect . Height , posRect . Width , posRect . Height ), $" { theItem . Name } \n { factOfferPerMin }/{ needNumPerMin } " , uiLabelStyle );
            return  isClick ;
        }
        public  override  int  GetItemId ()
        {
            return  theItem . ID ;
        }
        public  static  bool  isNeedChangeRecipe  =  false ;
        
        public  void  UpdateMultiItem ()
        {
            MultiUpdateCallback ? . The Invoke ( the this );
        }
        public  override  void  UpdateTree ( int  parentItemId )
        {
            // Initialization
            treeHight  =  1 ;
            treeWidth  =  1 ;
            // Raw materials/ Self-supplied
            if ( TheItem . recipes . Count  ==  0  ||  recipeIndex  ==  - 1 ) {
                inputNodes . Clear ();
                LeafUpdateCallback ? . The Invoke ( the this );
                return ;
            }
            // Module provides
            if ( outputNodes . Count  !=  0  && (( RootItem ) myRootNode ). outSet . Contains ( theItem . ID )) {
                inputNodes . Clear ();
                recipeIndex  =  - . 1 ;
                LeafUpdateCallback ? . The Invoke ( the this );
                return ;
            }

            Factory  inputFactory ;
            if ( inputNodes . Count  >  0 ) {
                inputFactory  = ( Factory ) inputNodes [ 0 ];
                if ( inputFactory . theRecipe . ID  !=  theItem . recipes [ recipeIndex ]. ID ) {
                    inputFactory  =  new  Factory ( TheItem . recipes [ recipeIndex ]);
                }
            }
            else {
                inputFactory  =  new  Factory ( TheItem . recipes [ recipeIndex ]);
            }
            inputFactory . outputFindChart [ theItem . ID ] =  this ;
            inputFactory . Layer  =  Layer  +  . 1 ;
            inputFactory . myRootNode  =  myRootNode ;
            if ( inputNodes . Count  >  0 ) {
                inputNodes [ 0 ] =  inputFactory ;
            }
            else {
                inputNodes . Add ( inputFactory );
            }
            inputFactory . UpdateTree ( theItem . ID );
            treeHight  =  inputFactory . treeHight  +  1 ;
            treeWidth  =  inputFactory . treeWidth ;
        }

        public  override  void  StatisticsDownResult ()
        {
            factOfferPerMin  =  needNumPerMin ;
            //The input factory will only exist
            if ( inputNodes . Count  >  0 ) {
                ( inputNodes [ 0 ] as  Factory ). StatisticsDownResult ();
            }
            // Leaf node statistics input
            else {
                Statistics ? . The Invoke ( the this );
            }
        }
    }




}