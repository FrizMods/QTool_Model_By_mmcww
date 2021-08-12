using  System ;
using  System . Collections . Generic ;
using  System . Linq ;
using  System . Text ;
using  UnityEngine ;

namespace  Qtools
{
    public  class  UITree
    {
        // =============================================== ============Graphic format parameters
        /// < summary >
        /// Grid spacing parameter 2
        /// </ summary >
        protected  float  line  =  2 ;
        /// < summary >
        /// Grid size
        /// </ summary >
        protected  float  side  =  40 ;
        /// < summary >
        /// UI displays the maximum number of grids per line parameter 40
        /// </ summary >
        protected  float  max  =  40 ;
        /// < summary >
        /// Boundary distance parameter 10
        /// </ summary >
        protected  float  leftAndRight  =  10 ;
        /// < summary >
        /// Top margin parameter 25
        /// </ summary >
        private  float  topOfMap  =  25 ;
        // ========================== Window parameters
        private  Rect  windowRect ;
        public  Rect  maxWindowRect ;
        public  Vector2  windowPosition  =  new  Vector2 ( 0 , 0 );
        private  bool  mouseLock  =  false ;
        private  Vector3  lastMousePosition ;

        public  Material  lineMaterial ;
        /// < summary >
        ///The auxiliary array index is the layer label
        /// </ summary >
        private  float [] posRight ;

        // =============================== UI events
        public  TreeNode  nowActiveNode ;
        public  bool  isClickAnyNode  =  false ;

        public  float [] PosRight { get  =>  posRight ; set  =>  posRight  =  value ;}
        public  Rect  WindowRect { get  =>  windowRect ; set {
                windowRect  =  value ;
                if ( maxWindowRect  ==  null ) {
                    maxWindowRect  =  new  Rect ( windowRect );
                }
                else {
                    maxWindowRect . x  =  windowRect . x ;
                    maxWindowRect . y  =  windowRect . y
;}

            }
        }

        Action < TreeNode > uIPressEvent ;

        public  UITree ()
        {
            uIPressEvent  = ( x ) => {
                nowActiveNode  =  x ;
                isClickAnyNode  =  true ;
            };
        }








        /// < summary >
        /// Draw a tree
        /// </ summary >
        /// < param  name = " theNode " >Currently drawn node</ param >
        /// < param  name = " minPosX " >The minimum abscissa position of the leftmost leaf node of the tree</ param >
        private  void  DrawTree ( TreeNode  theNode , float  minPosX )
        {
            // Post-order traversal
            // The minimum abscissa position of the current icon
            float  theMin  =  minPosX  >  PosRight [ theNode . layer ] ?  minPosX  :  PosRight [ theNode . layer ];
            if ( theNode . inputNodes  !=  null  &&  theNode . inputNodes . Count  >  0 ) {
                // Subtree
                // Determine the adjacent nodes of adjacent subtrees  
                if ( PosRight [ theNode . layer ] >  PosRight [ theNode . layer  +  1 ] +  line ) {
                    // Approximately balanced node structure
                    a float  TEMP  = ( theNode . treeWidth  -  . 1 ) *  0 . . 5 F  * ( Side  +  Line );
                    // Estimate
                    // if (theNode.treeWidth != theNode.inputNodes.Count) {
                    //     temp += temp*(theNode.treeWidth-theNode.inputNodes.Count) * 0.02f;
                    // }
                    // Correct the position of the subtree
                    minPosX  =  theMin  -  temp ; // posRight[theNode.layer]-temp;
                }
                // Judgment by-product-position correction
                if ( theNode . inputNodes [ 0 ]. outputNodes . Count  >  1 ) { // theNode.GetType().Name == "ItemRoad" &&
                    if ( minPosX  <=  PosRight [ theNode . layer ]) {
                        minPosX  =  minPosX  + ( theNode . inputNodes [ 0 ]. outputNodes . the Count  -  . 1 ) *  0 . . 5 F  * ( Side  +  Line );
                    }
                }
                DrawTree ( theNode . InputNodes [ 0 ], minPosX );
                for ( int  i  =  1 ; i  <  theNode . inputNodes . Count ; i ++ ) {
                    TreeNode  node  =  theNode . InputNodes [ i ];
                    DrawTree ( node , PosRight [ theNode . Layer  +  1 ]);
                }
                // Intermediate node
                float  ans  = ( theNode . inputNodes [ theNode . inputNodes . Count  -  1 ]. posRect . x  +  theNode . inputNodes [ 0 ]. posRect . x ) /  2 ;
                // Consider by-products
                if ( theNode . inputNodes [ 0 ]. outputNodes . Count  >  1 ) { // ((theNode is ItemRoad)||(theNode is RootItem)) &&
                    ANS  - = ( theNode . inputNodes [ 0 ]. outputNodes . the Count  -  . 1 ) *  0 . . 5 F  * ( Side  +  Line );
                    PosRight [ theNode . Layer ] =  ANS  >  posRight [ theNode . Layer ] ?  ANS  :  posRight [ theNode . Layer ];
                    foreach ( TreeNode  road  in  theNode . inputNodes [ 0 ]. outputNodes ) {
                        // Approximate position correction
                        road . posRect . x  =  PosRight [ theNode . layer ];
                        PosRight [ theNode . Layer ] =  PosRight [ theNode . Layer ] +  side  +  line ;
                        ConnectLine ( theNode . InputNodes [ 0 ], road );
                    }
                }
                else {
                    // Approximate position correction
                    ans  =  ans  >  PosRight [ theNode . layer ] ?  ans  :  PosRight [ theNode . layer ];
                    theNode . posRect . x  =  ans ;
                    PosRight [ theNode . Layer ] =  theNode . PosRect . X  +  side  +  line ;

                }
                if ( theNode . outputNodes . Count  !=  0 ) {
                    ConnectLine ( theNode , theNode . OutputNodes [ 0 ]);
                }

            }
            else {
                //The position of the leaf node is determined by the minimum horizontal coordinate allowed by the parent node and the minimum horizontal coordinate allowed by the current layer
                // Judging by-products
                theNode . posRect . x  =  theMin ;
                PosRight [ theNode . Layer ] =  theNode . PosRect . X  +  side  +  line ;
                // Maybe the root node is the leaf node
                if ( theNode . outputNodes . Count  >  0 ) {
                    ConnectLine ( theNode , theNode . OutputNodes [ 0 ]);
                }

            }
            // Set location
            // Draw the UI
            // Draw multiple products
            if ( theNode . inputNodes . Count  >  0  &&  theNode . inputNodes [ 0 ]. outputNodes . Count  >  1 ) { // && theNode.GetType().Name == "ItemRoad"
                foreach ( TreeNode  road  in  theNode . inputNodes [ 0 ]. outputNodes ) {
                    // Approximate position correction

                    road . posRect . y  =  theNode . layer  *  2  * ( side  +  line ) +  topOfMap ;
                    road . posRect . height  =  side ;
                    road . posRect . width  =  side ;
                    if ( road . UIDraw ()) {
                        isClickAnyNode  =  true ;
                        nowActiveNode  =  road ;
                    }

                }
            }
            else {
                theNode . posRect . y  =  theNode . layer  *  2  * ( side  +  line ) +  topOfMap ;
                theNode . posRect . height  =  side ;
                theNode . posRect . width  =  side ;
                if ( theNode . UIDrawUIDraw ()) {
                    isClickAnyNode  =  true ;
                    nowActiveNode  =  theNode ;
                }
            }
            // Update the auxiliary array
            // posRight[theNode.layer] = theNode.posRect.x + side + line;
        }



        public  void  DrawAllTrees ( TreeManage  treeManage )
        {
            if ( treeManage . rootItems . Count  ==  0 ) {
                return ;
            }
            PosRight  =  new  float [ treeManage . RootItems . Max < RootItem >(). treeHight ];
            for ( int  i  =  0 ; i  <  PosRight . Length ; i ++ ) {
                PosRight [ i ] =  leftAndRight  +  windowRect . X ;
            }
            windowPosition  =  GUI . BeginScrollView ( windowRect , windowPosition , maxWindowRect );
            foreach ( TreeNode  node  in  treeManage . rootItems ) {
                DrawTree ( node , PosRight [ node . Layer ]);
            }
            GUI . EndScrollView ();
            if ( nowActiveNode  !=  null ) {
                DrawBorder ( nowActiveNode , Color . Red );
                // =================== Root node change formula special operation
                if ( ItemRoad . isNeedChangeRecipe ) {
                    ItemRoad . IsNeedChangeRecipe  =  false ;
                    // treeManage.ChangeRecipe(nowActiveNode);
                    nowActiveNode  =  treeManage . ChangeRecipe ( nowActiveNode );
                    // isClickAnyNode = false;
                }
                if ( TreeNode . isNeedToModule ) {
                    TreeNode . IsNeedToModule  =  false ;
                    treeManage . ToModule ( nowActiveNode );
                    nowActiveNode  =  null ;
                    isClickAnyNode  =  false ;
                }
            }
            // Update the maximum canvas size
            maxWindowRect . width  =  0 ;
            maxWindowRect . height  = (( PosRight . Length  +  2 ) * ( side  +  line )) *  2 ;
            foreach ( int  t  in  posRight ) {
                if ( t  >  maxWindowRect . width ) {
                    maxWindowRect . width  =  t ;
                }
            }
            maxWindowRect . width  += ( side  +  line ) *  5  -  leftAndRight  *  2 ;
            maxWindowRect . width  =  windowRect . width  >  maxWindowRect . width  ?  windowRect . width  :  maxWindowRect . width ;
            maxWindowRect . height  =  windowRect . height  >  maxWindowRect . height  ?  windowRect . height  :  maxWindowRect . height ;



            if ( windowRect . Contains ( Input . mousePosition )) {

                // ==== In response to the mouse wheel, the icon zooms in and out
                max  -= ( int ) Input . mouseScrollDelta . y ;
                max  =  max  <  15  ?  15  :  max ;
                max  =  max  >  52  ?  52  :  max ;
                Side  = ( windowRect . width  -  2  *  leftAndRight  - ( max  -  . 1 ) *  Line ) /  max ;
                The TreeNode . UiButtonStyle . The fontSize  = ( int ) ( Side  /  . 5 );
                The TreeNode . UiLabelStyle . The fontSize  = ( int ) ( Side  /  . 5 );


                // ============ Right click and drag the canvas
                if ( Input . GetMouseButtonDown ( 1 ) &&  ! mouseLock ) {
                    mouseLock  =  true ;
                    lastMousePosition  =  Input . mousePosition ;
                }
                if ( Input . GetMouseButtonUp ( 1 ) &&  mouseLock ) {
                    mouseLock  =  false ;
                }
                if ( Input . GetMouseButton ( 1 ) &&  mouseLock ) {
                    Vector2  offset  =  Input . MousePosition  -  lastMousePosition ;
                    offset . X  =  - offset . X ;
                    windowPosition  +=  offset ;
                    lastMousePosition  =  Input . mousePosition ;
                    // QTools.QTool.DebugLog($"now{windowPosition.x}/{windowPosition.y}");
                }
                windowPosition . x  =  windowPosition . x  >  maxWindowRect . width  -  windowRect . width  ?  maxWindowRect . width  -  windowRect . width  :  windowPosition . x ;
                windowPosition . x  =  windowPosition . x  <  0  ?  0  :  windowPosition . x ;
                windowPosition . y  =  windowPosition . y  >  maxWindowRect . height  -  windowRect . height  ?  maxWindowRect . height  -  windowRect . height  :  windowPosition . y ;
                windowPosition . y  =  windowPosition . y  <  0  ?  0  :  windowPosition . y ;
            }
        }
        /// < summary >
        /// theNode1 and theNode2 are connected to the bottom
        /// [Features] Only the visibility of the x-axis is judged
        /// </ summary >
        /// < param  name = " parentNode " ></ param >
        private  void  ConnectLine ( TreeNode  theNode1 , TreeNode  theNode2 )
        {
            float  x1 , x2 , y1 , y2 ;
            // ============ Coordinate system transformation
            y1  =  theNode1 . posRect . y ;
            y2  =  theNode2 . posRect . y  +  side ;
            x1  =  theNode1 . posRect . x  +  side  /  2 ;
            x2  =  theNode2 . posRect . x  +  side  /  2 ;

            // ============== Coordinate transformation
            x1  =  windowRect . x  +  x1  -  windowPosition . x ;
            x2  =  windowRect . x  +  x2  -  windowPosition . x ;
            y1  =  windowRect . y  +  y1  -  windowPosition . y ;
            y2  =  windowRect . y  +  y2  -  windowPosition . y ;
            // ==============y-axis flip
            y1  =  windowRect . height  -  y1 ;
            y2  =  windowRect . height  -  y2 ;
            // ==============Visible area limitation
            x1  =  windowRect . x  +  windowRect . width  <  x1  ?  windowRect . x  +  windowRect . width  :  x1 ;
            x2  =  windowRect . x  +  windowRect . width  <  x2  ?  windowRect . x  +  windowRect . width  :  x2 ;
            GL . PushMatrix (); // Save the current Matirx
            lineMaterial . SetColor ( " _Color " , Color . black );
            lineMaterial . SetPass ( 0 ); // Refresh the current material
            GL . LoadPixelMatrix (); // Set pixelMatrix
            GL . Begin ( GL . LINES );
            GL . Vertex3 ( x1 , y1 , 0 );
            GL . Vertex3 ( x1 , ( y1  +  y2 ) /  2 , 0 );
            GL . Vertex3 ( x1 , ( y1  +  y2 ) /  2 , 0 );
            GL . Vertex3 ( x2 , ( y1  +  y2 ) /  2 , 0 );
            GL . Vertex3 ( x2 , ( y1  +  y2 ) /  2 , 0 );
            GL . Vertex3 ( x2 , y2 , 0 );

            GL . End ();
            GL . PopMatrix (); // Read the previous Matrix
        }
        /// < summary >
        /// [Characteristics] Only the visibility judgment is made on the right side
        /// </ summary >
        /// < param  name = " theNode " ></ param >
        /// < param  name = " color " ></ param >
        private  void  DrawBorder ( TreeNode  theNode , Color  color )
        {
            Rect  theRect  =  new  Rect ( theNode . PosRect );
            // ============= Coordinate transformation
            theRect . x  -=  windowPosition . x ;
            theRect . y  -=  windowPosition . y ;

            Vector3  p1 , p2 , p3 , p4 ;
            bool  flag  =  true ;
            float  theSide  =  line ;
            P1  =  new new  Vector3 ( theRect . X  -  theSide , windowRect . height  - ( theRect . Y  -  theSide ), 0 );
            if ( p1 . x  >  windowRect . x  +  windowRect . width ) {
                return ;
            }
            P2  =  new new  Vector3 ( theRect . xMax  +  theSide , windowRect . height  - ( theRect . Y  -  theSide ), 0 );
            P3  =  new new  Vector3 ( theRect . xMax  +  theSide , windowRect . height  - ( theRect . yMax  +  theSide ), 0 );
            if ( p2 . x  >  windowRect . x  +  windowRect . width ) {
                flag  =  false ;
                p2 . x  =  windowRect . width  +  windowRect . x ;
                p3 . x  =  p2 . x ;
            }
            P4  =  new new  Vector3 ( theRect . X  -  theSide , windowRect . height  - ( theRect . yMax  +  theSide ), 0 );
            GL . PushMatrix (); // Save the current Matirx
            lineMaterial . SetColor ( " _Color " , color );
            lineMaterial . SetPass ( 0 ); // Refresh the current material
            GL . LoadPixelMatrix (); // Set pixelMatrix
            GL . Begin ( GL . LINES );
            // ===1
            GL . Vertex ( p1 );
            GL . Vertex ( p2 );
            // ===2
            if ( flag ) {
                GL . Vertex ( p2 );
                GL . Vertex ( p3 );
            }
            // ===3
            GL . Vertex ( p3 );
            GL . Vertex ( p4 );
            // ===4
            GL . Vertex ( p4 );
            GL . Vertex ( p1 );

            GL . End ();
            GL . PopMatrix (); // Read the previous Matrix
        }
    }
    /// < summary >
    /// Algorithm
    /// Recording algorithm-Start end
    /// Apply object GetRect and child Layout
    /// </ summary >
    public  class  MyGUILayout
    {
        private  int  topline  =  20 ;
        private  int  leftAndRight  =  10 ;
        private  Rect  nowParentRect ;

        private  int  nowPositionY = 0 ;
        private  int  nowPositionX = 0 ;
        private  int  nowLayerHeight  =  0 ;

        public  MyGUILayout ( Rect  parentRect )
        {
            SetParentRect ( parentRect );
        }
        public  MyGUILayout  SetLeftAndRight ( int  leftAndRight )
        {
            this . leftAndRight  =  leftAndRight ;
            return  this ;
        }
        public  MyGUILayout  SetTopLine ( int  topline )
        {
            this . topline  =  topline ;
            return  this ;
        }

        public  MyGUILayout  SetParentRect ( Rect  parentRect )
        {
            nowParentRect  =  parentRect ;
            nowPositionY  = ( int )( topline + parentRect . y );
            nowPositionX  =  0 ;
            return  this ;
        }
        /// < summary >
        /// 
        /// </ summary >
        /// < param  name = " sizeScale " > Ratio relative to the width of the parent range</ param >
        /// < returns ></ returns >
        public  MyGUILayout  MoveNextRow ( float  sizeScale , int  yline = 3 )
        {
            nowPositionX  =  leftAndRight ;
            nowPositionY  +=  nowLayerHeight  +  yline ;
            nowLayerHeight  = ( int )(( nowParentRect . width - 2 * leftAndRight ) *  sizeScale );
            return  this ;
        }
        public  MyGUILayout  Start ( float  sizeScale )
        {
            nowLayerHeight  = ( int )(( nowParentRect . width  -  2  *  leftAndRight ) *  sizeScale );
            nowPositionY  = ( int )( topline  +  nowParentRect . y );
            nowPositionX  =  leftAndRight ;
            return  this ;
        }

        /// < summary >
        /// 
        /// </ summary >
        /// < param  name = " sizeScale " > Ratio relative to the width of the parent range 0=All the rest of this line</ param >
        /// < param  name = " leftLine " >Left margin</ param >
        /// < returns ></ returns >
        public  Rect  MoveNext ( float  sizeScale  =  0 , int  leftLine = 0 )
        {
            nowPositionX  +=  leftLine ;
            float  width  = ( sizeScale  ==  0 ) ? ( nowParentRect . width  -  nowPositionX  -  leftAndRight  -  leftLine ) : ( sizeScale  * ( nowParentRect . width  -  2  *  leftAndRight ));
            Rect  ansRect  =  new  Rect ( nowPositionX , nowPositionY , width , nowLayerHeight );
            // QTools.QTool.DebugLog($"x:{nowPositionX},y:{nowPositionY},width:{width},heigh:{nowLayerHeight}");
            nowPositionX  += ( int ) width ;
            return  ansRect ;
        }
        public  Rect  GetUsedRect ()
        {
            Rect  ansRect  =  new  Rect ();
            ansRect . y  =  nowParentRect . y ;
            ansRect . x  =  0 ;
            if ( nowPositionX  >  leftAndRight ) {
                ansRect . height  =  nowPositionY  +  nowLayerHeight ;

            }
            else {
                ansRect . height  =  nowPositionY ;
            }
            ansRect . width  =  nowParentRect . width ;
            return  ansRect ;
        }
        public  Rect  GetRemainingRect ()
        {
            Rect  ansRect  =  new  Rect ();
            if ( nowPositionX  >  leftAndRight ) {
                ansRect . y  =  nowPositionY  +  nowLayerHeight ;

            }
            else {
                ansRect . y  =  nowPositionY ;
            }
            ansRect . x  =  0 ;
            ansRect . width  =  nowParentRect . width ;
            ansRect . height  =  nowParentRect . height  -  ansRect . y ;
            return  ansRect ;
        }

        public  int  GetSize ( float  sizeScale , int  num  =  0 , int  yline = 3 ) {
            return ( int )(( sizeScale  * ( nowParentRect . width  -  2  *  leftAndRight ) +  yline ) *  num );
        }

    }

}