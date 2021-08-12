using  System ;
using  BepInEx ;
using  UnityEngine ;
using  HarmonyLib ;
using  System . Collections ;
using  Qtools ;
using  System . Collections . Generic ;

namespace  QTools
{
    [ BepInPlugin ( " me.sky.plugin.Dyson.QTools " , " QTools " , " 3.0 " )]
    public  class  QTool : BaseUnityPlugin
    {
        static  QTool  _instance ;

        Rect  windowRect  =  new  Rect ( 0 , 0 , 0 , 0 ); // Window
        Rect  windowInformationRect  =  new  Rect ( 0 , 0 , 0 , 0 ); // Window
        Vector2  scorllPosition0  =  new  Vector2 ( 0 , 0 );
        Vector2  scorllPosition1  =  new  Vector2 ( 0 , 0 );
        Vector2  scorllPosition2  =  new  Vector2 ( 0 , 0 );
        private  int  selectId = 0 ;
        private  int  inoutSelect  =  1 ;
        Private  a float  sizeScaleFont  =  0 . 07 F ;
        Private  a float  sizeScalePic  =  0 . . 17 F ;
        Private  a float  sizeScaleLabel  =  0 . 12 is F ;

        /// < summary >
        /// Keyboard lock flag
        /// </ summary >
        bool  keyLock  =  false ;
        bool  isShowInformation  =  true ;
        /// < summary >
        /// Previous mouse position
        /// </ summary >
        // Vector3 lastMousePosition;
        Hashtable  translateMap  =  new  Hashtable ();
        Dictionary < int , List < ItemProto >> itemProductChart  =  new  Dictionary < int , List < ItemProto >>();
        
        /// < summary >
        /// Boundary distance parameter 10
        /// </ summary >
        float  leftAndRight  =  10 ;
        bool  showGUI  =  false ;
        bool  dataLoadOver  =  false ;
        
        Material  lineMaterial ;

        GUIStyle  qButtonStyle ;
        GUIStyle  qLabelStyle ;
        UITree  uITree ;
        TreeManage  treeManage ;
        MyGUILayout  myLayout ;

        public  static  void  DebugLog ( string  str )
        {
            _instance . Logger . LogInfo ( str );
        }

        void  CreateLineMaterial ()
        {
            if ( ! lineMaterial ) {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader  shader  =  Shader . Find ( " Hidden/Internal-Colored " );
                lineMaterial  =  new  Material ( shader );
                lineMaterial . hideFlags  =  HideFlags . HideAndDontSave ;
                // Turn on alpha blending
                lineMaterial . SetInt ( " _SrcBlend " , ( int ) UnityEngine . Rendering . BlendMode . SrcAlpha );
                lineMaterial . SetInt ( " _DstBlend " , ( int ) UnityEngine . Rendering . BlendMode . OneMinusSrcAlpha );
                // Turn backface culling off
                lineMaterial . SetInt ( " _Cull " , ( int ) UnityEngine . Rendering . CullMode . Off );
                // Turn off depth writes
                lineMaterial . SetInt ( " _ZWrite " , 0 );
                lineMaterial . SetColor ( 0 , Color . black );
            }
        }
        private  void  UpdateWindowSize ()
        {
            windowRect . height  =  Screen . height ;
            windowInformationRect . height  =  Screen . height ;
            if ( isShowInformation ) {
                windowInformationRect . width  = ( int ) ( Screen . width * 0 . 12 is ) +  leftAndRight  *  2 ;
                myLayout  =  new  MyGUILayout ( windowInformationRect );
            }
            else {
                windowInformationRect . width  =  0 f ;
            }
            windowRect . width  =  Screen . width  -  windowInformationRect . width ;
            windowInformationRect . x  =  windowRect . width  +  windowRect . x ;

            uITree . WindowRect  =  windowRect ;
        }
        private  void  Start ()
        {
            _instance  =  this ;
            new  Harmony ( " mmcww " ). PatchAll ();
            // Harmony.CreateAndPatchAll(typeof(QTools), null);
            InitTranslateMap ();
            uITree  =  new  UITree ();
            treeManage  =  new  TreeManage ();
        }
        void  OnGUI ()
        {
            if ( Input . GetKeyDown ( KeyCode . BackQuote ) &&  ! keyLock )
            {
                keyLock  =  true ;
                if ( ! showGUI )
                {
                    showGUI  =  true ;
                    qButtonStyle  =  new  GUIStyle ( GUI . skin . button );
                    qButtonStyle . fontSize  = ( int )( windowInformationRect . width  *  sizeScaleFont );
                    qLabelStyle  =  new  GUIStyle ( GUI . skin . label );
                    qLabelStyle . fontSize  = ( int )( windowInformationRect . width  *  sizeScaleFont );
                    The TreeNode . UiButtonStyle  =  new new  GUIStyle ( the GUI . Skin . Button );
                    The TreeNode . UiLabelStyle  =  new new  GUIStyle ( the GUI . Skin . Label );
                    CreateLineMaterial ();
                    uITree . lineMaterial  =  lineMaterial ;
                    if ( UIRoot . instance  !=  null )
                    {
                        UIRoot . Instance . OpenLoadingUI ();
                    }

                    // ================================== Window parameter initialization
                    UpdateWindowSize ();
                    Color  theColor  =  GUI . Color ;
                    theColor . a  =  0 ;
                }
                else
                {
                    isShowInformation  =  true ;
                    showGUI  =  false ;
                    if ( UIRoot . instance  !=  null )
                    {
                        UIRoot . Instance . CloseLoadingUI ();
                    }
                    itemProductChart . Clear ();
                }
                
            }
            if ( Input . GetKeyUp ( KeyCode . BackQuote ) &&  keyLock )
            {
                keyLock  =  false ;
            }
            if ( showGUI )
            {
                // ==== show two windows
                windowRect  =  GUI . Window ( 1 , windowRect , drawWindow , " QTools " );
                if ( isShowInformation ) {
                    windowInformationRect  =  GUI . Window ( 0 , windowInformationRect , drawInformation , " Message " );
                    myLayout . SetParentRect ( windowInformationRect );
                }
            }
        }

        

        void  drawInformation ( int  WindowID )
        {
            if ( GameMain . instance  !=  null  ||  dataLoadOver ) {
                dataLoadOver  =  true ;
            }
            else {
                return ;
            }
            if ( isShowInformation ) {
                string [] toolbarTexts  =  new  string [] { " Items " , " Information " , " Statistics " };
                myLayout . Start ( sizeScaleLabel );
                // ================================== Reset button
                if ( GUI . Button ( myLayout . MoveNext (), " Empty " , qButtonStyle )) {
                    treeManage . ClearTopItem ();
                    uITree . nowActiveNode  =  null ;
                }
                // ===================================== Reset shortcut keys
                if ( Input . GetKeyUp ( KeyCode . Backspace ) &&  ! keyLock ) {
                    treeManage . ClearTopItem ();
                    uITree . nowActiveNode  =  null ;
                }
                // =================================== Bus sorting
                if ( GUI . Button ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), " Built Sorting " , qButtonStyle )) {
                    treeManage . sortEvent . Set ();
                }
                if ( uITree . isClickAnyNode ) {
                    uITree . isClickAnyNode  =  false ;
                    selectId  =  1 ;
                    // Logger.LogInfo("select change");
                }
                // ================================== page selection button
                selectId  =  GUI . Toolbar ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), selectId , toolbarTexts , qButtonStyle );
                Vector3  mousePos  =  Input . MousePosition ;
                mousePos . y  =  windowInformationRect . height  -  mousePos . y ;
                Rect  rect  =  myLayout . GetUsedRect ();
                rect . x  +=  windowInformationRect . x ;
                rect . y  +=  windowInformationRect . y ;
                if ( rect . Contains ( mousePos )) {
                    // ===================== Respond to the mouse wheel to switch quickly
                    int  t  = ( int ) Input . mouseScrollDelta . y ;
                    selectId  = ( selectId  +  t ) %  3 ;
                    if ( selectId  <  0 ) {
                        selectId  =  2 ;
                    }

                }

                // =========================Calculation shortcut keys
                if ( Input . GetKeyUp ( KeyCode . Return ) &&  ! keyLock ) {
                    treeManage . calEvent . Set ();
                }
                switch ( selectId ) {
                    // ====================================== draw all items with formula
                    case  0 : {
                            int  i  =  0 ;
                            Rect  disRect  =  myLayout . GetRemainingRect ();
                            Rect  maxRect  =  new  Rect ( disRect );
                            maxRect . height  =  myLayout . GetSize ( 0 . . 19 F , LDB . items . dataArray is . the Length  /  . 5  +  . 5 );
                            scorllPosition2  =  GUI . BeginScrollView ( disRect , scorllPosition2 , maxRect );
                            foreach ( ItemProto  tempItemProto  in  LDB . items . dataArray ) {
                                if ( tempItemProto . recipes . Count  >  0 ) {
                                    if ( i  %  5  ==  0 ) {
                                        myLayout . MoveNextRow ( 0 . . 19 F );
                                    }
                                    IF ( the GUI . the Button ( myLayout . the MoveNext ( 0 . . 19 F ), tempItemProto . iconSprite . Texture , qButtonStyle )) {
                                        RootItem  itemRoad  =  new  RootItem ( tempItemProto . ID );
                                        treeManage . AddTopItem ( itemRoad );
                                       // Logger.LogInfo($"now item id ={tempItemProto.ID}");
                                    }
                                    i ++ ;
                                }
                            }
                            GUI . EndScrollView ();
                        }
                        break ;
                    case  1 : {
                            // ===========================Draw the processing logic UI of the selected item
                            if ( uITree . nowActiveNode  ==  null ) {
                                break ;
                            }
                            // ========================== draw the current item
                            myLayout . MoveNextRow ( sizeScalePic );
                            The GUI . The Label ( myLayout . MoveNext ( 0 . 4 f ), " the current selection: " , qLabelStyle );
                            The GUI . Box ( myLayout . MoveNext ( sizeScalePic ), LDB . Items . The Select ( uITree . NowActiveNode . GetItemId . ()) IconSprite . The Texture , qButtonStyle );
                            

                            // ======================= Only the root node can be deleted
                            if ( uITree . nowActiveNode . layer == 0 ) {
                                if ( GUI . Button ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), " Delete " , qButtonStyle )) {
                                    treeManage . DeleteTopItem ( uITree . nowActiveNode );
                                    uITree . nowActiveNode  =  null ;
                                    break ;
                                }
                                // ============================== delete shortcut keys
                                if ( Input . GetKeyDown ( KeyCode . Delete ) &&  ! keyLock ) {
                                    treeManage . DeleteTopItem ( uITree . nowActiveNode );
                                    uITree . nowActiveNode  =  null ;
                                    break ;
                                }
                            }
                            // =========================Non-root node, non-leaf node can be modularized formula
                            if ( uITree . nowActiveNode . inputNodes . Count  >  0  &&  uITree . nowActiveNode . outputNodes . Count  >  0 ) {
                                if ( GUI . Button ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), " Modular Formula " , qButtonStyle )) {
                                    treeManage . ToModule ( uITree . nowActiveNode );
                                    uITree . nowActiveNode  =  null ;
                                    break ;
                                }
                                // ========================= Modular shortcut keys
                                // if (Input.GetKeyUp(KeyCode.Space) && !keyLock) {
                                //     uITree.nowActiveNode.ToModule();
                                //     uITree.nowActiveNode = null;
                                //     break;
                                // }
                            }
                            // =========================== Factory specific information
                            if ( uITree . nowActiveNode  is  Factory ) {
                                Factory  theFactory  = ( Factory ) uITree . NowActiveNode ;
                                // ======================== If it is a manufacturing station, you can switch
                                if ( theFactory . FactoryId  >=  2303  &&  theFactory . FactoryId  <=  2305 ) {
                                    if ( GUI . Button ( myLayout . MoveNextRow ( sizeScaleFont ). MoveNext (), " Switch Manufacturing Station " , qButtonStyle )) {
                                        theFactory . ChangFactory ();
                                    }
                                }
                                // =======================Draw the formula of the current factory
                                // ==========Product
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), " Product: " , qLabelStyle );
                                myLayout . MoveNextRow ( sizeScalePic );
                                for ( int  i  =  0 ; i  <  theFactory . theRecipe . Results . Length ; i ++ ) {
                                    int  theItemId  =  theFactory . theRecipe . Results [ i ];
                                    if ( GUI . Button ( myLayout . MoveNext ( sizeScalePic ),
                                        LDB . Items . The Select ( theItemId ). IconSprite . Texture , qButtonStyle )) {
                                        uITree . nowActiveNode  =  theFactory . outputFindChart [ theItemId ];
                                    }
                                }
                                The GUI . The Label ( myLayout . MoveNextRow ( sizeScaleLabel .) The MoveNext ( 0 . . 4 F ), " material " , qLabelStyle );
                                // =========== Ingredients
                                myLayout . MoveNextRow ( sizeScalePic );
                                for ( int  i  =  0 ; i  <  theFactory . theRecipe . Items . Length ; i ++ ) {
                                    int  theItemId  =  theFactory . theRecipe . Items [ i ];
                                    if ( GUI . Button ( myLayout . MoveNext ( sizeScalePic ),
                                        LDB . Items . The Select ( theItemId ). IconSprite . Texture , qButtonStyle )) {
                                        uITree . nowActiveNode  =  theFactory . inputFindChart [ theItemId ];
                                    }
                                }
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $ " Efficiency: {( int )( theFactory . Efficiency  *  100 f )}% " , qLabelStyle );
                                System . Text . StringBuilder  sb  =  new  System . Text . StringBuilder ( "          " , 12 );
                                ItemProto  theItem  =  LDB . Items . The Select ( theFactory . FactoryId );
                                StringBuilderUtility . WriteKMG ( sb , 8 ,
                                    ( long )(( theItem . prefabDesc . workEnergyPerTick  *  theFactory . Efficiency  + ( 1  -  theFactory . Efficiency ) *  theItem . prefabDesc . idleEnergyPerTick )
                                      *  TheFactory . The Num  *  60 L ), to true );
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext ()
                                    , $" Total rated power: { sb . ToString (). TrimStart ( new  char [ 0 ])}W " , qLabelStyle );
                                StringBuilderUtility . WriteKMG ( sb , 8 , theItem . PrefabDesc . IdleEnergyPerTick  *  theFactory . Num  *  60 L , true );
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext ()
                                    , $" Total standby power: { sb . ToString (). TrimStart ( new  char [ 0 ])}W " , qLabelStyle );
                            }
                            // ================================ Conveyor belt
                            else {
                                ItemRoad  theRoad  = ( ItemRoad ) uITree . NowActiveNode ;
                                if ( GUI . Button ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), " Switch Formula " , qButtonStyle )) {
                                    treeManage . ChangeRecipe ( uITree . nowActiveNode );
                                }
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $ " Provide: { theRoad . FactOfferPerMin } / Demand: { theRoad . NeedNumPerMin } " , qLabelStyle );
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext ()
                                    , " Product " , qLabelStyle );
                                if ( ! itemProductChart . ContainsKey ( theRoad . GetItemId ())) {
                                    itemProductChart [ theRoad . GetItemId ()] =  GetAllProduct ( theRoad . GetItemId ());
                                }
                                int  i  =  0 ;
                                foreach ( ItemProto  tempItemProto  in  itemProductChart [ theRoad . GetItemId ()]) {
                                    if ( i  %  5  ==  0 ) {
                                        myLayout . MoveNextRow ( sizeScalePic );
                                    }
                                    IF ( the GUI . the Button ( myLayout . the MoveNext ( 0 . 2 F ), tempItemProto . iconSprite . Texture , qButtonStyle )) {
                                        RootItem  itemRoad  =  new  RootItem ( tempItemProto . ID );
                                        treeManage . AddTopItem ( itemRoad );
                                    }
                                    i ++ ;
                                }
                                if ( theRoad . layer == 0 ) {
                                    RootItem  theTop  = ( RootItem ) theRoad . MyRootNode ;
                                    int  yi  =  theTop . outItems [ theRoad ][ 0 ];
                                    // Actual output
                                    The GUI . The Label ( myLayout . MoveNextRow ( sizeScaleLabel .) MoveNext ( 0 . 3 f ), " demand: " , qButtonStyle );
                                    // Additional requirements
                                    int . the TryParse ( the GUI . the TextField ( myLayout . the MoveNext ( 0 . . 3 F ), Yi . the ToString (), . 6 , qButtonStyle ), OUT  Yi );
                                    theTop . outItems [ theRoad ][ 0 ] =  yi ;
                                    IF ( the GUI . the Button ( myLayout . the MoveNext ( 0 . . 3 F ), " confirmation " , qButtonStyle )) {
                                        treeManage . calEvent . Set ();
                                        // treeManage.StatisticsDownResult();
                                    }

                                    if ( theTop . ansPower  !=  null ) {
                                        System . Text . StringBuilder  sb  =  new  System . Text . StringBuilder ( "          " , 12 );
                                        StringBuilderUtility . WriteKMG ( sb , 8 , theTop . AnsPower [ 0 ], true );
                                        GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $" Maximum power consumption: { sb . ToString (). TrimStart ( new  char [ 0 ])}W " , qLabelStyle );
                                        sb  =  new  System . Text . StringBuilder ( "          " , 12 );
                                        StringBuilderUtility . WriteKMG ( sb , 8 , theTop . AnsPower [ 1 ], true );
                                        GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $" Rated power consumption: { sb . ToString (). TrimStart ( new  char [ 0 ])}W " , qLabelStyle );
                                        sb  =  new  System . Text . StringBuilder ( "          " , 12 );
                                        StringBuilderUtility . WriteKMG ( sb , 8 , theTop . AnsPower [ 2 ], true );
                                        GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $" Standby power consumption: { sb . ToString (). TrimStart ( new  char [ 0 ])}W " , qLabelStyle );
                                        GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $" sorter (reference): { theTop . AnsPicker }pieces " , qLabelStyle );
                                    }
                                    if ( theTop . ansFactory  !=  null ) {
                                        foreach ( int  factorId  in  theTop . ansFactory . Keys ) {
                                            The GUI . Box ( myLayout . MoveNextRow ( sizeScalePic .) MoveNext ( sizeScalePic ), LDB . Items . The Select ( factorId .) IconSprite . The Texture , qButtonStyle );
                                            GUI . Label ( myLayout . MoveNext (), $" { theTop . AnsFactory [ factorId ]} " , qLabelStyle );
                                        }
                                    }
                                }

                            }
                        }
                        break ;
                    case  2 : {
                            string [] toolbarTexts2  =  new  string [] { " raw material " , " product " , " cost " };
                            inoutSelect  =  GUI . Toolbar ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), inoutSelect , toolbarTexts2 , qButtonStyle );
                            if ( inoutSelect  ==  0 ) {
                                // =================== Input table Rect maxRect = new Rect(leftAndRight, ypos, windowInformationRect.width-10-leftAndRight, 0);

                                Rect  disPlayEra  =  myLayout . GetRemainingRect ();
                                Rect  maxRect  =  new  Rect ( disPlayEra . X , disPlayEra . Y , windowInformationRect . Width  -  2  *  leftAndRight , myLayout . GetSize ( sizeScalePic , treeManage . AnsNeedInput . Count + 5 ));
                                // if (maxRect.height <(windowInformationRect.height-ypos-10)) {
                                //     maxRect.height = windowInformationRect.height-ypos-10;
                                // }
                                // Rect disPlayEra2 = new Rect(disPlayEra);
                                // disPlayEra2.x += windowInformationRect.x;
                                // disPlayEra2.y += windowInformationRect.y;
                                // if (disPlayEra2.Contains(Input.mousePosition)) {
                                //     mouseScrollLock = true;
                                //     scorllPosition0.y += Input.mouseScrollDelta.y;
                                //     scorllPosition0.y = scorllPosition0.y <0? 0: scorllPosition0.y;
                                //     scorllPosition0.y = scorllPosition0.y> maxRect.height-disPlayEra.height? maxRect.height-disPlayEra.height: scorllPosition0.y;
                                // }
                                scorllPosition0 = GUI . BeginScrollView ( disPlayEra   , scorllPosition0   , maxRect );
                                foreach ( int  itemId  in  treeManage . ansNeedInput . Keys ) {
                                    if ( treeManage . ansNeedInput [ itemId ] <=  0 ) {
                                        continue ;
                                    }
                                    The GUI . Box ( myLayout . MoveNextRow ( sizeScalePic .) MoveNext ( sizeScalePic ), LDB . Items . The Select ( itemId .) IconSprite . The Texture , qButtonStyle );
                                    int  yi  = ( int )( treeManage . ansNeedInput [ itemId ]);
                                    GUI . Label ( myLayout . MoveNext (), $" { yi } / min " , qButtonStyle );
                                }
                                GUI . EndScrollView ();
                            }
                            else  if ( inoutSelect  ==  1 ) {
                                // ======== output table
                                if ( GUI . Button ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), " Calculate " , qButtonStyle )) {
                                    treeManage . calEvent . Set ();
                                        // treeManage.StatisticsDownResult();
                                }
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), " Output/min | Demand/min | Ratio% " , qButtonStyle );

                                Rect  disPlayEra  =  myLayout . GetRemainingRect ();
                                Rect  maxRect  =  new  Rect ( disPlayEra . X , disPlayEra . Y , windowInformationRect . Width   -  2  *  leftAndRight , myLayout . GetSize ( sizeScalePic , treeManage . AnsOutCount + 5 ));
                                // if (maxRect.height <(windowInformationRect.height-ypos-10)) {
                                //     maxRect.height = windowInformationRect.height-ypos-10;
                                // }
                                // Rect disPlayEra2 = new Rect(disPlayEra);
                                // disPlayEra2.x += windowInformationRect.x;
                                // disPlayEra2.y += windowInformationRect.y;
                                // if (disPlayEra2.Contains(Input.mousePosition)) {
                                //     mouseScrollLock = true;
                                //     scorllPosition1.y += Input.mouseScrollDelta.y;
                                //     scorllPosition1.y = scorllPosition1.y <0? 0: scorllPosition1.y;
                                //     scorllPosition1.y = scorllPosition1.y> maxRect.height-disPlayEra.height? maxRect.height-disPlayEra.height: scorllPosition1.y;
                                // }
                                scorllPosition1  =  the GUI . BeginScrollView ( disPlayEra , scorllPosition1 , maxRect );
                                foreach ( RootItem  rootItem  in  treeManage . rootItems ) {
                                    foreach ( ItemRoad  outItem  in  rootItem . outItems . Keys ) {
                                        if ( GUI . Button ( myLayout . MoveNextRow ( sizeScalePic ). MoveNext ( sizeScalePic ), outItem . TheItem . iconSprite . texture , qButtonStyle )) {
                                            uITree . nowActiveNode  =  outItem ;
                                        }
                                        int  yi  =  rootItem . outItems [ outItem ][ 0 ];
                                        int  yi2  =  rootItem . outItems [ outItem ][ 1 ];
                                        int  temp2  =  yi2 ;
                                        // Actual output
                                        The GUI . The Label ( myLayout . The MoveNext ( 0 . . 3 F ), $ " {( int ) ( outItem . FactOfferPerMin )} " , qButtonStyle );
                                        // Additional requirements
                                        int . the TryParse ( the GUI . the TextField ( myLayout . the MoveNext ( 0 . . 3 F ), Yi . the ToString (), . 6 , qButtonStyle ), OUT  Yi );
                                        rootItem . outItems [ outItem ][ 0 ] =  yi ;
                                        // Proportion
                                        int . TryParse ( GUI . TextField ( myLayout . MoveNext ( sizeScalePic ), yi2 . ToString (), 6 , qButtonStyle ), out  yi2 );
                                        if ( temp2  !=  yi2 ) {
                                            rootItem . outItems [ outItem ][ 1 ] =  yi2 ;
                                        }
                                    }
                                }
                                GUI . EndScrollView ();

                            }
                            // Display cost
                            else {

                                // =======================Total rated power consumption
                                System . Text . StringBuilder  sb  =  new  System . Text . StringBuilder ( "          " , 12 );
                                StringBuilderUtility . WriteKMG ( sb , 8 , treeManage . AnsPower [ 0 ], true );
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $" Maximum power consumption: { sb . ToString (). TrimStart ( new  char [ 0 ])}W " , qLabelStyle );
                                sb  =  new  System . Text . StringBuilder ( "          " , 12 );
                                StringBuilderUtility . WriteKMG ( sb , 8 , treeManage . AnsPower [ 1 ], true );
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $" Rated power consumption: { sb . ToString (). TrimStart ( new  char [ 0 ])}W " , qLabelStyle );
                                sb  =  new  System . Text . StringBuilder ( "          " , 12 );
                                StringBuilderUtility . WriteKMG ( sb , 8 , treeManage . AnsPower [ 2 ], true );
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $" Standby power consumption: { sb . ToString (). TrimStart ( new  char [ 0 ])}W " , qLabelStyle );
                                GUI . Label ( myLayout . MoveNextRow ( sizeScaleLabel ). MoveNext (), $ " sorter (reference): { treeManage . AnsPicker } a " , qLabelStyle );
                                foreach ( int  factorId  in  treeManage . ansFactory . Keys ) {
                                    The GUI . Box ( myLayout . MoveNextRow ( sizeScalePic .) MoveNext ( sizeScalePic ), LDB . Items . The Select ( factorId .) IconSprite . The Texture , qButtonStyle );
                                    GUI . Label ( myLayout . MoveNext (), $" { treeManage . AnsFactory [ factorId ]}个" , qLabelStyle );
                                    
                                }

                            }

                        }
                        break ;
                    default : {

                        }
                        break ;
                }

            }
        }
        private  List < ItemProto > GetAllProduct ( int  itemId ) {
            List < ItemProto > ans  =  new  List < ItemProto >();
            foreach ( ItemProto  tempItemProto  in  LDB . items . dataArray ) {
                bool  flag  =  false ;
                if ( tempItemProto . recipes . Count  >  0 ) {
                    foreach ( RecipeProto  recipeProto  in  tempItemProto . recipes ) {
                        foreach ( int  theId  in  recipeProto . Items ) {
                            if ( theId  ==  itemId ) {
                                ans . Add ( tempItemProto );
                                flag  =  true ;
                                break ;
                            }
                        }
                        if ( flag ) {
                            break ;
                        }
                    }
                }
            }
            return  ans ;
        }
        void  drawWindow ( int  WindowID )
        {
            if ( GameMain . instance  !=  null  ||  dataLoadOver )
            {
                dataLoadOver  =  true ;
                // =======
                uITree . DrawAllTrees ( treeManage );
            }
            else
            {
                GUI . Label ( new  Rect ( 10 , 20 , windowRect . Width , windowRect . Height ), Translate ( " Waiting for game resources to load! " ));
            }
            // ====
            string  stText  =  " < " ;
            if ( isShowInformation ) {
                stText  =  " > " ;
            }
            Rect  RECT  =  new new  Rect ( windowInformationRect . X  -  20 is , windowRect . Height  *  0 . . 5 F , 20 is , 40 );
            if ( GUI . Button ( rect , stText , qButtonStyle )) {
                isShowInformation  =  ! isShowInformation ;
                UpdateWindowSize ();
            }
            // ========================== Shortcut keys for side information window
            if ( Input . GetKeyUp ( KeyCode . Q ) &&  ! keyLock ) {
                isShowInformation  =  ! isShowInformation ;
                UpdateWindowSize ();
            }

        }
        
        void  InitTranslateMap ()
        {
            translateMap . Clear ();
            translateMap . Add ( " Waiting for game resources to load! " , " Wait for game resources to load! " );
            translateMap . Add ( " Close " , " Close " );
            translateMap . Add ( " Amount " , " Amount " );
            translateMap . Add ( " Return " , " Return " );
            translateMap . Add ( " raw material " , " raw " );
            translateMap . Add ( " self-provided " , " bySelf " );
            translateMap . Add ( " byproduct " , " byProducts " );
            translateMap . Add ( " target " , " target " );
            translateMap . Add ( " temporary " , " temp " );
            translateMap . Add ( " processing factory " , " factory " );
            translateMap . the Add ( " Power " , " Power Consumption " );
            translateMap . Add ( " unknown " , " unknown " );
        }
        public  static  string  Translate ( string  text )
        {
            if ( _instance . translateMap . ContainsKey ( text ) &&  Localization . language  !=  Language . zhCN )
            {
                return  _instance . translateMap [ text ]. ToString ();
            }
            else
            {
                return  text ;
            }
        }
    }
}