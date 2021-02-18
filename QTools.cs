using System;
using BepInEx;
using UnityEngine;
using HarmonyLib;
using System.Collections;
using Qtools;
using System.Collections.Generic;

namespace QTools
{
    [BepInPlugin("me.sky.plugin.Dyson.QTools", "QTools", "3.0")]
    public class QTool : BaseUnityPlugin
    {
        static QTool _instance;

        Rect windowRect = new Rect(0, 0, 0, 0);//窗口
        Rect windowInformationRect = new Rect(0, 0, 0, 0);//窗口
        Vector2 scorllPosition0 = new Vector2(0, 0);
        Vector2 scorllPosition1 = new Vector2(0, 0);
        Vector2 scorllPosition2 = new Vector2(0, 0);
        private int selectId=0;
        private int inoutSelect = 1;
        private float sizeScaleFont = 0.07f;
        private float sizeScalePic = 0.17f;
        private float sizeScaleLabel = 0.12f;

        /// <summary>
        /// 键盘锁定flag
        /// </summary>
        bool keyLock = false;
        bool isShowInformation = true;
        /// <summary>
        /// 上一个鼠标位置
        /// </summary>
        //Vector3 lastMousePosition;
        Hashtable translateMap = new Hashtable();
        Dictionary<int, List<ItemProto>> itemProductChart = new Dictionary<int, List<ItemProto>>();
        
        /// <summary>
        /// 边界距离 参数 10
        /// </summary>
        float leftAndRight = 10;
        bool showGUI = false;
        bool dataLoadOver = false;
        
        Material lineMaterial;

        GUIStyle qButtonStyle;
        GUIStyle qLabelStyle;
        UITree uITree;
        TreeManage treeManage;
        MyGUILayout myLayout;

        public static void DebugLog(string str) 
        {
            _instance.Logger.LogInfo(str);
        }

        void CreateLineMaterial()
        {
            if (!lineMaterial) {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
                lineMaterial.SetColor(0, Color.black);
            }
        }
        private void UpdateWindowSize()
        {
            windowRect.height = Screen.height;
            windowInformationRect.height = Screen.height;
            if (isShowInformation) {
                windowInformationRect.width = (int)(Screen.width*0.12) + leftAndRight * 2;
                myLayout = new MyGUILayout(windowInformationRect);
            }
            else {
                windowInformationRect.width = 0f;
            }
            windowRect.width = Screen.width - windowInformationRect.width;
            windowInformationRect.x = windowRect.width + windowRect.x;

            uITree.WindowRect = windowRect;
        }
        private void Start()
        {
            _instance = this;
            new Harmony("mmcww").PatchAll();
            //Harmony.CreateAndPatchAll(typeof(QTools), null);
            InitTranslateMap();
            uITree = new UITree();
            treeManage = new TreeManage();
        }
        void OnGUI()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote) && !keyLock)
            {
                keyLock = true;
                if (!showGUI)
                {
                    showGUI = true;
                    qButtonStyle = new GUIStyle(GUI.skin.button);
                    qButtonStyle.fontSize = (int)(windowInformationRect.width * sizeScaleFont);
                    qLabelStyle = new GUIStyle(GUI.skin.label);
                    qLabelStyle.fontSize = (int)(windowInformationRect.width * sizeScaleFont);
                    TreeNode.uiButtonStyle = new GUIStyle(GUI.skin.button);
                    TreeNode.uiLabelStyle = new GUIStyle(GUI.skin.label); 
                    CreateLineMaterial();
                    uITree.lineMaterial = lineMaterial;
                    if (UIRoot.instance != null)
                    {
                        UIRoot.instance.OpenLoadingUI();
                    }

                    //==================================窗口参数初始化
                    UpdateWindowSize();
                    Color theColor = GUI.color;
                    theColor.a = 0;
                }
                else
                {
                    isShowInformation = true;
                    showGUI = false;
                    if (UIRoot.instance != null)
                    {
                        UIRoot.instance.CloseLoadingUI();
                    }
                    itemProductChart.Clear();
                }
                
            }
            if (Input.GetKeyUp(KeyCode.BackQuote) && keyLock)
            {
                keyLock = false;
            }
            if (showGUI)
            {
                //====显示两个窗口
                windowRect = GUI.Window(1, windowRect, drawWindow, "QTools");
                if (isShowInformation) {
                    windowInformationRect = GUI.Window(0, windowInformationRect, drawInformation, "Message");
                    myLayout.SetParentRect(windowInformationRect);
                }
            }
        }

        

        void drawInformation(int WindowID)
        {
            if (GameMain.instance != null || dataLoadOver) {
                dataLoadOver = true;
            }
            else {
                return;
            }
            if (isShowInformation) {
                string[] toolbarTexts = new string[] { "物品", "信息", "统计" };
                myLayout.Start(sizeScaleLabel);
                //===================================重置按钮
                if (GUI.Button(myLayout.MoveNext(), "清空", qButtonStyle)) {
                    treeManage.ClearTopItem();
                    uITree.nowActiveNode = null;
                }
                //======================================重置快捷键
                if (Input.GetKeyUp(KeyCode.Backspace) && !keyLock) {
                    treeManage.ClearTopItem();
                    uITree.nowActiveNode = null;
                }
                //===================================总线化排序
                if (GUI.Button(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), "总线化排序", qButtonStyle)) {
                    treeManage.SortLevel(true);
                }
                if (uITree.isClickAnyNode) {
                    uITree.isClickAnyNode = false;
                    selectId = 1;
                    //Logger.LogInfo("select change");
                }
                //===================================页选按钮
                selectId = GUI.Toolbar(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), selectId, toolbarTexts, qButtonStyle);
                Vector3 mousePos = Input.mousePosition;
                mousePos.y = windowInformationRect.height - mousePos.y;
                Rect rect = myLayout.GetUsedRect();
                rect.x += windowInformationRect.x;
                rect.y += windowInformationRect.y;
                if (rect.Contains(mousePos)) {
                    //=====================响应鼠标滚轮快捷切换
                    int t = (int)Input.mouseScrollDelta.y;
                    selectId = (selectId + t) % 3;
                    if (selectId < 0) {
                        selectId = 2;
                    }

                }

                //==========================计算快捷键
                if (Input.GetKeyUp(KeyCode.Return) && !keyLock) {
                    treeManage.StatisticsDownResult();
                }
                switch (selectId) {
                    //======================================绘制全部有配方物品
                    case 0: {
                            int i = 0;
                            Rect disRect = myLayout.GetRemainingRect();
                            Rect maxRect = new Rect(disRect);
                            maxRect.height = myLayout.GetSize(0.19f, LDB.items.dataArray.Length / 5 + 5);
                            scorllPosition2 = GUI.BeginScrollView(disRect, scorllPosition2, maxRect);
                            foreach (ItemProto tempItemProto in LDB.items.dataArray) {
                                if (tempItemProto.recipes.Count > 0) {
                                    if (i % 5 == 0) {
                                        myLayout.MoveNextRow(0.19f);
                                    }
                                    if (GUI.Button(myLayout.MoveNext(0.19f), tempItemProto.iconSprite.texture, qButtonStyle)) {
                                        RootItem itemRoad = new RootItem(tempItemProto.ID);
                                        treeManage.AddTopItem(itemRoad);
                                       // Logger.LogInfo($"now item id ={tempItemProto.ID}");
                                    }
                                    i++;
                                }
                            }
                            GUI.EndScrollView();
                        }
                        break;
                    case 1: {
                            //===========================绘制选中物品的处理逻辑UI
                            if (uITree.nowActiveNode == null) {
                                break;
                            }
                            //===========================绘制当前物品
                            myLayout.MoveNextRow(sizeScalePic);
                            GUI.Label(myLayout.MoveNext(0.4f), "当前选中:", qLabelStyle);
                            GUI.Box(myLayout.MoveNext(sizeScalePic), LDB.items.Select(uITree.nowActiveNode.GetItemId()).iconSprite.texture, qButtonStyle);
                            

                            //=======================仅根节点可删除
                            if (uITree.nowActiveNode.layer==0) {
                                if (GUI.Button(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), "删除", qButtonStyle)) {
                                    treeManage.DeleteTopItem(uITree.nowActiveNode);
                                    uITree.nowActiveNode = null;
                                    break;
                                }
                                //==============================删除快捷键
                                if (Input.GetKeyDown(KeyCode.Delete) && !keyLock) {
                                    treeManage.DeleteTopItem(uITree.nowActiveNode);
                                    uITree.nowActiveNode = null;
                                    break;
                                }
                            }
                            //========================非根节点，非叶节点可模块化配方
                            if (uITree.nowActiveNode.inputNodes.Count > 0 && uITree.nowActiveNode.outputNodes.Count > 0) {
                                if (GUI.Button(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), "模块化配方", qButtonStyle)) {
                                    treeManage.ToModule(uITree.nowActiveNode);
                                    uITree.nowActiveNode = null;
                                    break;
                                }
                                //==========================模块化快捷键
                                //if (Input.GetKeyUp(KeyCode.Space) && !keyLock) {
                                //    uITree.nowActiveNode.ToModule();
                                //    uITree.nowActiveNode = null;
                                //    break;
                                //}
                            }
                            //===========================工厂特有信息
                            if (uITree.nowActiveNode is Factory) {
                                Factory theFactory = (Factory)uITree.nowActiveNode;
                                //========================如果是制造台，可以切换
                                if (theFactory.FactoryId >= 2303 && theFactory.FactoryId <= 2305) {
                                    if (GUI.Button(myLayout.MoveNextRow(sizeScaleFont).MoveNext(), "切换制造台", qButtonStyle)) {
                                        theFactory.ChangFactory();
                                    }
                                }
                                //=======================绘制当前工厂的配方
                                //==========产物
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), "产物:", qLabelStyle);
                                myLayout.MoveNextRow(sizeScalePic);
                                for (int i = 0; i < theFactory.theRecipe.Results.Length; i++) {
                                    int theItemId = theFactory.theRecipe.Results[i];
                                    if (GUI.Button(myLayout.MoveNext(sizeScalePic),
                                        LDB.items.Select(theItemId).iconSprite.texture, qButtonStyle)) {
                                        uITree.nowActiveNode = theFactory.outputFindChart[theItemId];
                                    }
                                }
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(0.4f), "原料", qLabelStyle);
                                //===========原料
                                myLayout.MoveNextRow(sizeScalePic);
                                for (int i = 0; i < theFactory.theRecipe.Items.Length; i++) {
                                    int theItemId = theFactory.theRecipe.Items[i];
                                    if (GUI.Button(myLayout.MoveNext(sizeScalePic),
                                        LDB.items.Select(theItemId).iconSprite.texture, qButtonStyle)) {
                                        uITree.nowActiveNode = theFactory.inputFindChart[theItemId];
                                    }
                                }
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"效率:{(int)(theFactory.Efficiency * 100f)}%", qLabelStyle);
                                System.Text.StringBuilder sb = new System.Text.StringBuilder("         ", 12);
                                ItemProto theItem = LDB.items.Select(theFactory.FactoryId);
                                StringBuilderUtility.WriteKMG(sb, 8,
                                    (long)((theItem.prefabDesc.workEnergyPerTick * theFactory.Efficiency + (1 - theFactory.Efficiency) * theItem.prefabDesc.idleEnergyPerTick)
                                      * theFactory.Num * 60L), true);
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext()
                                    , $"总额定功率:{sb.ToString().TrimStart(new char[0])}W", qLabelStyle);
                                StringBuilderUtility.WriteKMG(sb, 8, theItem.prefabDesc.idleEnergyPerTick * theFactory.Num * 60L, true);
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext()
                                    , $"总待机功率:{sb.ToString().TrimStart(new char[0])}W", qLabelStyle);
                            }
                            //================================传送带
                            else {
                                ItemRoad theRoad = (ItemRoad)uITree.nowActiveNode;
                                if (GUI.Button(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), "切换配方",qButtonStyle)) {
                                    treeManage.ChangeRecipe(uITree.nowActiveNode);
                                }
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"提供:{theRoad.factOfferPerMin} / 需求:{theRoad.needNumPerMin}", qLabelStyle);
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext()
                                    , "产物", qLabelStyle);
                                if (!itemProductChart.ContainsKey(theRoad.GetItemId())) {
                                    itemProductChart[theRoad.GetItemId()] = GetAllProduct(theRoad.GetItemId());
                                }
                                int i = 0;
                                foreach (ItemProto tempItemProto in itemProductChart[theRoad.GetItemId()]) {
                                    if (i % 5 == 0) {
                                        myLayout.MoveNextRow(sizeScalePic);
                                    }
                                    if (GUI.Button(myLayout.MoveNext(0.2f), tempItemProto.iconSprite.texture, qButtonStyle)) {
                                        RootItem itemRoad = new RootItem(tempItemProto.ID);
                                        treeManage.AddTopItem(itemRoad);
                                    }
                                    i++;
                                }
                                if (theRoad.layer==0) {
                                    RootItem theTop = (RootItem)theRoad.myRootNode;
                                    int yi = theTop.outItems[theRoad][0];
                                    //实际产出
                                    GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(0.3f), "需求:", qButtonStyle);
                                    //额外需求
                                    int.TryParse(GUI.TextField(myLayout.MoveNext(0.3f), yi.ToString(), 6, qButtonStyle), out yi);
                                    theTop.outItems[theRoad][0] = yi;
                                    if (GUI.Button(myLayout.MoveNext(0.3f), "确认", qButtonStyle)) {
                                        treeManage.StatisticsDownResult();
                                    }

                                    if (theTop.ansPower != null) {
                                        System.Text.StringBuilder sb = new System.Text.StringBuilder("         ", 12);
                                        StringBuilderUtility.WriteKMG(sb, 8, theTop.ansPower[0], true);
                                        GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"最大功耗:{sb.ToString().TrimStart(new char[0])}W", qLabelStyle);
                                        sb = new System.Text.StringBuilder("         ", 12);
                                        StringBuilderUtility.WriteKMG(sb, 8, theTop.ansPower[1], true);
                                        GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"额定功耗:{sb.ToString().TrimStart(new char[0])}W", qLabelStyle);
                                        sb = new System.Text.StringBuilder("         ", 12);
                                        StringBuilderUtility.WriteKMG(sb, 8, theTop.ansPower[2], true);
                                        GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"待机功耗:{sb.ToString().TrimStart(new char[0])}W", qLabelStyle);
                                        GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"分拣器(参考):{theTop.ansPicker}个", qLabelStyle);
                                    }
                                    if (theTop.ansFactory != null) {
                                        foreach (int factorId in theTop.ansFactory.Keys) {
                                            GUI.Box(myLayout.MoveNextRow(sizeScalePic).MoveNext(sizeScalePic), LDB.items.Select(factorId).iconSprite.texture, qButtonStyle);
                                            GUI.Label(myLayout.MoveNext(), $"{theTop.ansFactory[factorId]}个", qLabelStyle);
                                        }
                                    }
                                }

                            }
                        }
                        break;
                    case 2: {
                            string[] toolbarTexts2 = new string[] { "原料", "产物","成本"};
                            inoutSelect = GUI.Toolbar(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), inoutSelect, toolbarTexts2, qButtonStyle);
                            if (inoutSelect == 0) {
                                //===================输入表Rect maxRect = new Rect(leftAndRight, ypos, windowInformationRect.width - 10 - leftAndRight, 0);

                                Rect disPlayEra = myLayout.GetRemainingRect();
                                Rect maxRect = new Rect(disPlayEra.x, disPlayEra.y, windowInformationRect.width - 2 * leftAndRight,myLayout.GetSize(sizeScalePic, treeManage.ansNeedInput.Count+5));
                                //if (maxRect.height < (windowInformationRect.height - ypos - 10)) {
                                //    maxRect.height = windowInformationRect.height - ypos - 10;
                                //}
                                //Rect disPlayEra2 = new Rect(disPlayEra);
                                //disPlayEra2.x += windowInformationRect.x;
                                //disPlayEra2.y += windowInformationRect.y;
                                //if (disPlayEra2.Contains(Input.mousePosition)) {
                                //    mouseScrollLock = true;
                                //    scorllPosition0.y += Input.mouseScrollDelta.y;
                                //    scorllPosition0.y = scorllPosition0.y < 0 ? 0 : scorllPosition0.y;
                                //    scorllPosition0.y = scorllPosition0.y > maxRect.height - disPlayEra.height ? maxRect.height - disPlayEra.height : scorllPosition0.y;
                                //}
                                scorllPosition0=GUI.BeginScrollView(disPlayEra  , scorllPosition0  , maxRect);
                                foreach (int itemId in treeManage.ansNeedInput.Keys) {
                                    if (treeManage.ansNeedInput[itemId] <= 0) {
                                        continue;
                                    }
                                    GUI.Box(myLayout.MoveNextRow(sizeScalePic).MoveNext(sizeScalePic), LDB.items.Select(itemId).iconSprite.texture, qButtonStyle);
                                    int yi = (int)(treeManage.ansNeedInput[itemId]);
                                    GUI.Label(myLayout.MoveNext(), $"{yi} / min", qButtonStyle);
                                }
                                GUI.EndScrollView();
                            }
                            else if (inoutSelect == 1) {
                                //========输出表
                                if (GUI.Button(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), "计算", qButtonStyle)) {
                                    treeManage.StatisticsDownResult();
                                }
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), "产出/min | 需求/min | 比例%", qButtonStyle);

                                Rect disPlayEra = myLayout.GetRemainingRect();
                                Rect maxRect = new Rect(disPlayEra.x, disPlayEra.y, windowInformationRect.width  - 2 * leftAndRight ,myLayout.GetSize(sizeScalePic, treeManage.ansOutCount+5));
                                //if (maxRect.height < (windowInformationRect.height - ypos - 10)) {
                                //    maxRect.height = windowInformationRect.height - ypos - 10;
                                //}
                                //Rect disPlayEra2 = new Rect(disPlayEra);
                                //disPlayEra2.x += windowInformationRect.x;
                                //disPlayEra2.y += windowInformationRect.y;
                                //if (disPlayEra2.Contains(Input.mousePosition)) {
                                //    mouseScrollLock = true;
                                //    scorllPosition1.y += Input.mouseScrollDelta.y;
                                //    scorllPosition1.y = scorllPosition1.y < 0 ? 0 : scorllPosition1.y;
                                //    scorllPosition1.y = scorllPosition1.y > maxRect.height - disPlayEra.height ? maxRect.height - disPlayEra.height : scorllPosition1.y;
                                //}
                                scorllPosition1 = GUI.BeginScrollView(disPlayEra , scorllPosition1 , maxRect );
                                foreach (RootItem rootItem in treeManage.rootItems) {
                                    foreach (ItemRoad outItem in rootItem.outItems.Keys) {
                                        if (GUI.Button(myLayout.MoveNextRow(sizeScalePic).MoveNext(sizeScalePic), outItem.TheItem.iconSprite.texture, qButtonStyle)) {
                                            uITree.nowActiveNode = outItem;
                                        }
                                        int yi = rootItem.outItems[outItem][0];
                                        int yi2 = rootItem.outItems[outItem][1];
                                        int temp2 = yi2;
                                        //实际产出
                                        GUI.Label(myLayout.MoveNext(0.3f), $"{(int)(outItem.factOfferPerMin)}", qButtonStyle);
                                        //额外需求
                                        int.TryParse(GUI.TextField(myLayout.MoveNext(0.3f), yi.ToString(), 6, qButtonStyle), out yi);
                                        rootItem.outItems[outItem][0] = yi;
                                        //比例
                                        int.TryParse(GUI.TextField(myLayout.MoveNext(sizeScalePic), yi2.ToString(), 6, qButtonStyle), out yi2);
                                        if (temp2 != yi2) {
                                            rootItem.outItems[outItem][1] = yi2;
                                        }
                                    }
                                }
                                GUI.EndScrollView();

                            }
                            //显示成本
                            else {

                                //=======================总额定功耗
                                System.Text.StringBuilder sb = new System.Text.StringBuilder("         ", 12);
                                StringBuilderUtility.WriteKMG(sb, 8,treeManage.ansPower[0], true);
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"最大功耗:{sb.ToString().TrimStart(new char[0])}W", qLabelStyle);
                                sb = new System.Text.StringBuilder("         ", 12);
                                StringBuilderUtility.WriteKMG(sb, 8, treeManage.ansPower[1], true);
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"额定功耗:{sb.ToString().TrimStart(new char[0])}W", qLabelStyle);
                                sb = new System.Text.StringBuilder("         ", 12);
                                StringBuilderUtility.WriteKMG(sb, 8, treeManage.ansPower[2], true);
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"待机功耗:{sb.ToString().TrimStart(new char[0])}W", qLabelStyle);
                                GUI.Label(myLayout.MoveNextRow(sizeScaleLabel).MoveNext(), $"分拣器(参考):{treeManage.ansPicker}个", qLabelStyle);
                                foreach (int factorId in treeManage.ansFactory.Keys) {
                                    GUI.Box(myLayout.MoveNextRow(sizeScalePic).MoveNext(sizeScalePic), LDB.items.Select(factorId).iconSprite.texture, qButtonStyle);
                                    GUI.Label(myLayout.MoveNext(), $"{treeManage.ansFactory[factorId]}个", qLabelStyle);
                                    
                                }

                            }

                        }
                        break;
                    default: {

                        }
                        break;
                }

            }
        }
        private List<ItemProto> GetAllProduct(int itemId) {
            List<ItemProto> ans = new List<ItemProto>();
            foreach (ItemProto tempItemProto in LDB.items.dataArray) {
                bool flag = false;
                if (tempItemProto.recipes.Count > 0) {
                    foreach (RecipeProto recipeProto in tempItemProto.recipes) {
                        foreach (int theId in recipeProto.Items) {
                            if (theId == itemId) {
                                ans.Add(tempItemProto);
                                flag = true;
                                break;
                            }
                        }
                        if (flag) {
                            break;
                        }
                    }
                }
            }
            return ans;
        }
        void drawWindow(int WindowID)
        {
            if (GameMain.instance != null || dataLoadOver)
            {
                dataLoadOver = true;
                //=======
                uITree.DrawAllTrees(treeManage);
            }
            else
            {
                GUI.Label(new Rect(10, 20, windowRect.width, windowRect.height), Translate("等待游戏资源加载!"));
            }
            //====
            string stText = "<";
            if (isShowInformation) {
                stText = ">";
            }
            Rect rect = new Rect(windowInformationRect.x - 20, windowRect.height * 0.5f, 20, 40);
            if (GUI.Button(rect, stText, qButtonStyle)) {
                isShowInformation = !isShowInformation;
                UpdateWindowSize();
            }
            //===========================侧面信息窗口快捷键
            if (Input.GetKeyUp(KeyCode.Q) && !keyLock) {
                isShowInformation = !isShowInformation;
                UpdateWindowSize();
            }

        }
        
        void InitTranslateMap()
        {
            translateMap.Clear();
            translateMap.Add("等待游戏资源加载!", "Wait for game resources to load!");
            translateMap.Add("关闭", "Close");
            translateMap.Add("数量", "Amount");
            translateMap.Add("返回", "Return");
            translateMap.Add("原料", "raw");
            translateMap.Add("自提供", "bySelf");
            translateMap.Add("副产物", "byProducts");
            translateMap.Add("目标", "target");
            translateMap.Add("临时物", "temp");
            translateMap.Add("加工厂", "factory");
            translateMap.Add("功耗", "Power Consumption");
            translateMap.Add("未知", "unknown");
        }
        public static string Translate(string text)
        {
            if (_instance.translateMap.ContainsKey(text) && Localization.language != Language.zhCN)
            {
                return _instance.translateMap[text].ToString();
            }
            else
            {
                return text;
            }
        }
    }
}