using Qtools;
using QTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Qtools
{
    /*
     * 确定树的高度和宽度
     * 绘制图形时生成辅助数组，记录每层最右端的最大坐标
     * 
     */
    /// <summary>
    /// <para>图的节点基类,服务于UI</para>
    /// </summary>
    public class TreeNode : IComparable<TreeNode>
    {
        /// <summary>
        /// UI位置
        /// </summary>
        public Rect posRect = new Rect();
        public static GUIStyle uiButtonStyle;
        public static GUIStyle uiLabelStyle;
        public static bool isNeedToModule = false;



        //================================================逻辑


        protected static Action<TreeNode> Statistics;

        /// <summary>
        /// 逻辑位置 树的高度 叶节点为1 自下而上 树建起时有效
        /// </summary>
        public int treeHight = 1;
        /// <summary>
        /// 逻辑位置 树的宽度 叶节点为1 自下而上 树建起时有效
        /// </summary>
        public int treeWidth = 1;
        /// <summary>
        /// 树的逻辑层标号 根节点为0 自上而下 树建起时有效
        /// </summary>
        public int layer = 0;

        public TreeNode myRootNode;

        /// <summary>
        /// itemId->传送带->数量/秒
        /// 输入 每种item只存在一条路径
        /// </summary>
        public List<TreeNode> inputNodes = new List<TreeNode>();
        /// <summary>
        /// itemId->传送带->数量/秒
        /// 输出 可能存在多个相同种类输出
        /// </summary>
        public List<TreeNode> outputNodes = new List<TreeNode>();

        public TreeNode()
        {
        }
        public TreeNode(TreeNode node) {

        }

        /// <summary>
        /// UI绘图
        /// </summary>
        /// <returns></returns>
        public virtual bool UIDraw()
        {
            return false;
        }
        public virtual int GetItemId()
        {
            return 0;
        }

        public virtual void UpdateTree(int parentItemId)
        { 
        
        }
        
        public virtual void StatisticsDownResult()
        {
        
        }
        public int CompareTo(TreeNode other)
        {
            if (this.treeHight > other.treeHight) {
                return 1;
            }
            else if (this.treeHight < other.treeHight) {
                return -1;
            }
            else
                return 0;
        }


        


        
    }
    /// <summary>
    /// 工厂 输出输入严格对应配方
    /// </summary>
    public class Factory : TreeNode
    {
        /// <summary>
        /// 工厂类型id
        /// </summary>
        private int factoryId;
        public RecipeProto theRecipe;
        /// <summary>
        /// 工厂效率
        /// </summary>
        private float efficiency = 1f;
        private float speed = 1f;
        /// <summary>
        /// 工厂数量
        /// </summary>
        private int num;

        public Dictionary<int, TreeNode> inputFindChart;
        public Dictionary<int, TreeNode> outputFindChart;

        public float Efficiency { get => efficiency; private set => efficiency = value; }
        public int FactoryId { get => factoryId; private set => factoryId = value; }
        public int Num { get => num; set => num = value; }

        /// <summary>
        /// 自动添加到allNodes中，speed默认1
        /// </summary>
        /// <param name="theRecipe"></param>
        public Factory(RecipeProto theRecipe) : base()
        {
            this.theRecipe = theRecipe;
            inputFindChart = new Dictionary<int, TreeNode>();
            outputFindChart = new Dictionary<int, TreeNode>();
            GetFactoryId(1f);
        }
        /// <summary>
        /// 获取工厂id
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        private int GetFactoryId(float speed)
        {
            switch (theRecipe.Type) {
                case ERecipeType.None:
                    break;
                case ERecipeType.Smelt:
                    factoryId = 2302;
                    break;
                case ERecipeType.Chemical:
                    factoryId = 2309;
                    break;
                case ERecipeType.Refine:
                    factoryId = 2308;
                    break;
                case ERecipeType.Assemble:
                    //三种制造台
                    if (speed == 0.75f) {
                        factoryId = 2303;
                    }
                    else if (speed == 1f) {
                        factoryId = 2304;
                    }
                    else if (speed == 1.5f) {
                        factoryId = 2305;
                    }
                    break;
                case ERecipeType.Particle:
                    factoryId = 2310;
                    break;
                case ERecipeType.Exchange:
                    factoryId = 2209;
                    break;
                case ERecipeType.PhotonStore:
                    factoryId = 2208;
                    break;
                case ERecipeType.Fractionate:
                    factoryId = 2314;
                    break;
                case ERecipeType.Research:
                    factoryId = 2901;
                    break;
            }
            return factoryId;
        }
        public override bool UIDraw()
        {
            bool isClick = false;
            if (GUI.Button(posRect, LDB.items.Select(factoryId).iconSprite.texture, uiButtonStyle)) {
                isClick = true;
                //制作台
                if (factoryId >= 2303 && factoryId <= 2305) {
                    if (Input.GetKey(KeyCode.LeftControl)) {
                        ChangFactory();
                    }
                }
                if (Input.GetKey(KeyCode.LeftAlt)) {
                    isNeedToModule = true;
                }

                //isUpdate = true;
            }
            GUI.Label(new Rect(posRect.x, posRect.y + posRect.height, posRect.width, posRect.height), $"{LDB.items.Select(factoryId).name}\n{QTool.Translate("数量")} : {num}", uiLabelStyle);
            return isClick;
        }
        public override int GetItemId()
        {
            return factoryId;
        }
        public void ChangFactory()
        {
            if (factoryId >= 2303 && factoryId <= 2305) {
                factoryId = (factoryId - 2303 + 1) % 3 + 2303;
            }
        }


        /// <summary>
        /// 【特性】只要工厂的配方不变，其子结点就不会改变 父节点变化或则父节点配方变化则此节点是一个新的结点
        /// 将会生成新的树
        /// </summary>
        /// <param name="offerInputRoads">已经提供的原材料</param>
        public override void UpdateTree(int parentItemId)
        {
            treeWidth = 0;
            treeHight = 0;
            float times = 0;
            for (int i = 0; i < theRecipe.Results.Length; i++) {
                if (theRecipe.Results[i] == parentItemId) {
                    times = ((ItemRoad)outputFindChart[parentItemId]).productTimes / theRecipe.ResultCounts[i];
                }
            }
            if (theRecipe.Type == ERecipeType.Fractionate) {
                times /= 100f;
            }

            //遍历原材料
            for (int i = 0; i < theRecipe.Items.Length; i++) {
                int itemId = theRecipe.Items[i];
                if (!inputFindChart.ContainsKey(itemId)) {
                    inputFindChart[itemId] = new ItemRoad(LDB.items.Select(itemId));
                    inputFindChart[itemId].outputNodes.Add(this);
                    inputFindChart[itemId].layer = layer + 1;
                    inputFindChart[itemId].myRootNode = myRootNode;
                    //初始化倍率
                    ((ItemRoad)inputFindChart[itemId]).productTimes = -times * theRecipe.ItemCounts[i];
                }
                (inputFindChart[itemId] as ItemRoad).UpdateTree(factoryId);
                treeWidth += inputFindChart[itemId].treeWidth;
                treeHight = treeHight > inputFindChart[itemId].treeHight ? treeHight : inputFindChart[itemId].treeHight;
            }
            treeHight += 1;
            //遍历产物
            for (int i = 0; i < theRecipe.Results.Length; i++) {
                int itemId = theRecipe.Results[i];
                if (!outputFindChart.ContainsKey(itemId)) {
                    outputFindChart[itemId] = new ItemRoad(LDB.items.Select(itemId));
                    outputFindChart[itemId].inputNodes.Add(this);
                    outputFindChart[itemId].myRootNode = myRootNode;
                    for (int ii = 0; ii < LDB.items.Select(itemId).recipes.Count; ii++){
                        if (LDB.items.Select(itemId).recipes[ii] == theRecipe) {
                            ((ItemRoad)outputFindChart[itemId]).recipeIndex = ii;
                            break;
                        }
                    }
                    outputFindChart[itemId].layer = layer - 1;
                    outputFindChart[itemId].treeWidth = treeWidth;
                    outputFindChart[itemId].treeHight = treeHight + 1;
                    //初始化倍率
                    ((ItemRoad)outputFindChart[itemId]).productTimes = times * theRecipe.ResultCounts[i];
                }
                if (outputFindChart[itemId].outputNodes.Count == 0) {
                    ((ItemRoad)outputFindChart[itemId]).UpdateMultiItem();
                }
            }
            outputNodes = outputFindChart.Values.ToList();
            inputNodes = inputFindChart.Values.ToList();
            //===============================排序
            //inputNodes.Sort();
        }

        public override void StatisticsDownResult()
        {
            double times = 0;
            //计算配方倍率
            for (int i = 0; i < theRecipe.Results.Length; i++) {
                double ttimes = (outputFindChart[theRecipe.Results[i]] as ItemRoad).needNumPerMin / theRecipe.ResultCounts[i];
                //按产能过剩优化
                times = times > ttimes ? times : ttimes;
            }
            double factCount = 0;
            //===========================特殊 蒸馏装置
            if (theRecipe.Type != ERecipeType.Fractionate) {
                factCount = times / speed / 60 / 60 * theRecipe.TimeSpend;

            }
            else {
                float productPer = 1800f / 100f;
                factCount = ((ItemRoad)outputFindChart[theRecipe.Results[0]]).needNumPerMin / productPer;
            }

            if ((int)factCount < factCount) {
                num = (int)factCount + 1;
                Efficiency = (float)(factCount / num);
            }
            else {
                num = (int)factCount;
                Efficiency = 1f;
            }
            Statistics?.Invoke(this);

            //计算实际输出
            for (int i = 0; i < theRecipe.Results.Length; i++) {
                (outputFindChart[theRecipe.Results[i]] as ItemRoad).factOfferPerMin = (float)(times * theRecipe.ResultCounts[i]);
            }
            //计算输入需求
            //===========================特殊 蒸馏装置
            if (theRecipe.Type != ERecipeType.Fractionate) {
                for (int i = 0; i < theRecipe.Items.Length; i++) {
                    (inputFindChart[theRecipe.Items[i]] as ItemRoad).needNumPerMin = (float)(times * theRecipe.ItemCounts[i]);
                    inputFindChart[theRecipe.Items[i]].StatisticsDownResult();
                }
            }
            else {
                ((ItemRoad)inputFindChart[theRecipe.Items[0]]).needNumPerMin = ((ItemRoad)outputFindChart[theRecipe.Results[0]]).needNumPerMin;
                inputFindChart[theRecipe.Items[0]].StatisticsDownResult();
            }
        }
    }
    /// <summary>
    /// 传输带模型，单输入，任意输出
    /// </summary>
    public class ItemRoad : TreeNode
    {
        /// <summary>
        /// 生产速率，规定>0为产出 <0为需求
        /// </summary>
        public float product = 0;
        /// <summary>
        /// 生产速率的倍率（顶端产出为60个/min时的生产量），规定>0为产出 <0为需求
        /// </summary>
        public float productTimes = 0;
        /// <summary>
        /// 传输带上的货物
        /// </summary>
        protected ItemProto theItem;
        /// <summary>
        /// 每分钟送货量
        /// </summary>
        public float needNumPerMin = 0;
        public float factOfferPerMin = 0;
        /// <summary>
        /// 配方索引 （-1 自提供）
        /// </summary>
        public int recipeIndex = 0;

        protected static Action<ItemRoad> LeafUpdateCallback;
        protected static Action<ItemRoad> MultiUpdateCallback;

        public ItemProto TheItem { get => theItem; private set => theItem = value; }
        public int RecipeIndex { get => recipeIndex; private set => recipeIndex = value; }

        public ItemRoad(int itemId) : base()
        {
            theItem = LDB.items.Select(itemId);
        }
        public ItemRoad(ItemProto theItem) : base()
        {
            this.TheItem = theItem;
        }
        public ItemRoad(ItemProto theItem, int recipeIndex) : base()
        {
            this.TheItem = theItem;
            this.recipeIndex = recipeIndex > theItem.recipes.Count ? 0 : recipeIndex;
        }
        public ItemRoad(ItemRoad road) : base(road)
        {
            theItem = road.theItem;
            recipeIndex = road.recipeIndex;
        }

        public override bool UIDraw()
        {
            bool isClick = false;
            if (TheItem.recipes.Count > 0) {
                if (GUI.Button(posRect, TheItem.iconSprite.texture, uiButtonStyle)) {
                    isClick = true;
                    if (Input.GetKey(KeyCode.LeftControl)) {
                        isNeedChangeRecipe = true;
                    }
                    else if (Input.GetKey(KeyCode.LeftAlt)) {
                        isNeedToModule = true;
                    }
                }
            }
            else {
                GUI.Box(posRect, TheItem.iconSprite.texture);
            }
            GUI.Label(new Rect(posRect.x, posRect.y + posRect.height, posRect.width, posRect.height), $"{theItem.name}\n{factOfferPerMin}/{needNumPerMin}", uiLabelStyle);
            return isClick;
        }
        public override int GetItemId()
        {
            return theItem.ID;
        }
        public static bool isNeedChangeRecipe = false;
        
        public void UpdateMultiItem()
        {
            MultiUpdateCallback?.Invoke(this);
        }
        public override void UpdateTree(int parentItemId)
        {
            //初始化
            treeHight = 1;
            treeWidth = 1;
            //原料 / 自提供 
            if (TheItem.recipes.Count == 0 || recipeIndex == -1) {
                inputNodes.Clear();
                LeafUpdateCallback?.Invoke(this);
                return;
            }
            // 模块提供
            if (outputNodes.Count != 0 && ((RootItem)myRootNode).outSet.Contains(theItem.ID)) {
                inputNodes.Clear();
                recipeIndex = -1;
                LeafUpdateCallback?.Invoke(this);
                return;
            }

            Factory inputFactory;
            if (inputNodes.Count > 0) {
                inputFactory = (Factory)inputNodes[0];
                if (inputFactory.theRecipe.ID != theItem.recipes[recipeIndex].ID) {
                    inputFactory = new Factory(TheItem.recipes[recipeIndex]);
                }
            }
            else {
                inputFactory = new Factory(TheItem.recipes[recipeIndex]);
            }
            inputFactory.outputFindChart[theItem.ID] = this;
            inputFactory.layer = layer + 1;
            inputFactory.myRootNode = myRootNode;
            if (inputNodes.Count > 0) {
                inputNodes[0] = inputFactory;
            }
            else {
                inputNodes.Add(inputFactory);
            }
            inputFactory.UpdateTree(theItem.ID);
            treeHight = inputFactory.treeHight + 1;
            treeWidth = inputFactory.treeWidth;
        }

        public override void StatisticsDownResult()
        {
            factOfferPerMin = needNumPerMin;
            //输入工厂只会唯一存在
            if (inputNodes.Count > 0) {
                (inputNodes[0] as Factory).StatisticsDownResult();
            }
            //叶节点统计输入
            else {
                Statistics?.Invoke(this);
            }
        }
    }




}
