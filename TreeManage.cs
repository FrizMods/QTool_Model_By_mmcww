using System;
using System.Collections.Generic;

namespace Qtools
{
    public class TreeManage
    {

        public List<RootItem> rootItems = new List<RootItem>();
        private HashSet<int> outSet = new HashSet<int>();

        //===============================================计算结果统计
        public Dictionary<int, float> ansNeedInput = new Dictionary<int, float>();
        /// <summary>
        /// [id] = 数量
        /// </summary>
        public Dictionary<int, int> ansFactory = new Dictionary<int, int>();
        /// <summary>
        /// [0]最大功耗 [1]额定功耗 [2]待机功耗
        /// </summary>
        public long[] ansPower = new long[3] { 0, 0, 0 };
        /// <summary>
        /// 爪子数量统计 三种分拣器id 2011 2012 2013
        /// </summary>
        public int ansPicker = 0;

        public int ansOutCount = 0;

        /// <summary>
        /// 添加产物
        /// 【特性】会更新输出表 输入表 更新所有树
        /// </summary>
        /// <param name="top"></param>
        public void AddTopItem(RootItem rootNode)
        {
            rootNode.SetInDebar(outSet).UpdateTree();
            ansOutCount += rootNode.outItems.Count;
            foreach (ItemRoad outNode in rootNode.outItems.Keys) {
                if (!outSet.Contains(outNode.TheItem.ID)) {
                    outSet.Add(outNode.TheItem.ID);
                }
            }
            foreach (RootItem rootItem in rootItems) {
                rootItem.SetInDebar(outSet).UpdateTree();
            }
            //更新根节点表
            rootItems.Add(rootNode);
            ////更新输出表 考虑多产物
            //Factory factory = (Factory)rootNode.inputNodes[0];
            ////删除其他树相同配方子结点
            //foreach (TreeNode topNode in rootItems) {
            //    DeleteDestinationRecipe(topNode, factory.theRecipe);
            //}
        }

        /// <summary>
        /// 将配方设置为模块
        /// 【特性】同时初始化模块参数列表
        /// 【限制】叶节点不能设置成模块
        /// </summary>
        public void ToModule(TreeNode theNode)
        {
            ItemRoad road;
            RecipeProto recipe;
            //无论当前node是road还是factory 获取其配方
            if (theNode is Factory) {
                road = theNode.outputNodes[0] as ItemRoad;
                recipe = ((Factory)theNode).theRecipe;
            }
            else if (theNode.inputNodes.Count > 0) {
                road = (ItemRoad)theNode;
                recipe = (theNode.inputNodes[0] as Factory).theRecipe;
            }
            else {
                return;
            }
            //最终产物（也可能是副产物）
            if (road.layer == 0) {
                return;
            }
            //根节点添加
            RootItem moduleRoad = new RootItem(road);
            AddTopItem(moduleRoad);
            //当前树结构改变
            //树结构改变 高度 宽度 ...
            //((RootItem)theNode.myRootNode).UpdateTree();
        }

        /// <summary>
        /// 切换配方
        /// <para>【限制】根节点不能切换成自提供 副产物不能改配方</para>
        /// </summary>
        /// <returns>true 有效切换/false 无效切换</returns>
        public TreeNode ChangeRecipe(TreeNode node)
        {
            ItemRoad theNode = node as ItemRoad;
            TreeNode ansNode;
            if (theNode != null) {
                //模块化的产物不能切换配方
                //if (outSet.Contains(theNode.TheItem.ID) && theNode.layer!=0) {
                //    return false;
                //}
                //=========根节点 不允许 配方为 无
                if (theNode.outputNodes.Count == 0) {
                    //副产物不能改配方
                    if (theNode.layer != 0) {
                        return theNode;
                    }
                    if (theNode.TheItem.recipes.Count > 1) {
                        //根节点切换配方和其它相同 多产物根节点切换配方将可能改变所有树的结构
                        theNode.recipeIndex++;
                        if (theNode.recipeIndex == theNode.TheItem.recipes.Count) {
                            theNode.recipeIndex = 0;
                        }
                        DeleteTopItem(theNode.myRootNode);
                        ansNode = new RootItem(theNode);
                        AddTopItem((RootItem)ansNode);
                        //继承原来的用户设置
                        ((RootItem)ansNode).outItems[(ItemRoad)ansNode][0] = ((RootItem)theNode.myRootNode).outItems[theNode][0];
                        ((RootItem)ansNode).outItems[(ItemRoad)ansNode][1] = ((RootItem)theNode.myRootNode).outItems[theNode][1];
                        return ansNode;
                    }
                    else {
                        return theNode;
                    }
                }
                //========其他节点
                else {
                    theNode.recipeIndex++;
                    if (theNode.recipeIndex == theNode.TheItem.recipes.Count) {
                        theNode.recipeIndex = -1;
                    }
                    //树结构改变 高度 宽度 ...
                    ((RootItem)theNode.myRootNode).UpdateTree();
                    return theNode;
                }
            }
            return theNode;
        }


        /// <summary>
        /// 删除root产物 删除原料表 
        /// </summary>
        /// <param name="top"></param>
        public void DeleteTopItem(TreeNode top)
        {
            if (top.layer==0) {
                RootItem topRoad = (RootItem)top.myRootNode;
                ansOutCount -= topRoad.outItems.Count;
                rootItems.Remove(topRoad);
                if (!IsOutHave(topRoad.TheItem.ID)) {
                    outSet.Remove(topRoad.TheItem.ID);
                }
            }
        }


        public void ClearTopItem()
        {
            rootItems.Clear();
            outSet.Clear();
            ansNeedInput.Clear();
            ansFactory.Clear();
            ansNeedInput.Clear();
            ansPicker = 0;
            ansPower[0] = 0;
            ansPower[1] = 0;
            ansPower[2] = 0;
            ansOutCount = 0;
        }

        /// <summary>
        /// 将产物树按输出等级高到低排序
        /// 【特性】多产物只添加rootNodes中存在的那种
        /// </summary>
        public List<RootItem> SortLevel(bool isLowToHigh = false)
        {
            //检查辅助矩阵
            Dictionary<RootItem, bool> checkArray = new Dictionary<RootItem, bool>();
            List<TreeNode> multiTop = new List<TreeNode>();
            //辅助矩阵初始化
            foreach (RootItem topNode in rootItems) {
                checkArray[topNode] = false;
            }
            //结果列表
            List<RootItem> theLevel = new List<RootItem>();
            //选择排序
            while (theLevel.Count < rootItems.Count) {
                // 对于一个根结点
                foreach (RootItem topNode in rootItems) {
                    //（过滤已经排序好的根节点）
                    if (checkArray[topNode])
                        continue;
                    bool isHave = false;
                    //的所有产物
                    foreach (ItemRoad outProduct in topNode.outItems.Keys) {
                        //===剩下根节点遍历
                        foreach (RootItem topNode1 in rootItems) {
                            //（过滤已经排序好的根节点和自己）
                            if (checkArray[topNode1]|| topNode1 == topNode)
                                continue;
                            foreach (ItemRoad inProduct in topNode1.inItems) {
                                //如果不在剩下的根节点原料列表中
                                if (outProduct.TheItem.ID == inProduct.TheItem.ID) {
                                    isHave = true;
                                    break;
                                }
                            }
                            if (isHave)
                                break;
                        }
                        if (isHave)
                            break;
                    }
                    //则该根节点的产物级别比剩下的根节点产物级别高
                    if (!isHave) {
                        checkArray[topNode] = true;
                        theLevel.Add(topNode);
                    }
                }
            }
            if (isLowToHigh) {
                theLevel.Reverse();
                rootItems = theLevel;
            }
            return theLevel;
        }
        /// <summary>
        /// 检查所有树的输出产物中是否有某种物品
        /// 【性能】最坏情乱O(n)
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool IsOutHave(int itemId)
        {
            foreach (RootItem topNode in rootItems) {
                foreach (ItemRoad outProduct in topNode.outItems.Keys) {
                    if (outProduct.TheItem.ID == itemId) {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 检查所有树的输入产物中是否有某种物品
        /// 【性能】最坏情乱O(n)
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool IsInHave(int itemId)
        {
            foreach (RootItem topNode in rootItems) {
                foreach (ItemRoad inProduct in topNode.inItems) {
                    if (inProduct.TheItem.ID == itemId) {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 获取生产特定产物的树
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public List<RootItem> GetTopItemNodes(int itemId)
        {
            List<RootItem> ans = new List<RootItem>();
            foreach (RootItem topNode in rootItems) {
                foreach (ItemRoad outProduct in topNode.outItems.Keys) {
                    if (outProduct.TheItem.ID == itemId) {
                        ans.Add(topNode);
                        break;
                    }
                }
            }
            return ans;
        }
        



        /// <summary>
        /// 不能删除根结点的配方树
        /// </summary>
        /// <param name="theNode"></param>
        /// <param name="recipeProto"></param>
        private static void DeleteDestinationRecipe(TreeNode theNode, RecipeProto recipeProto)
        {
            foreach (TreeNode node in theNode.inputNodes) {
                if (node.GetType().Name == "ItemRoad" && node.inputNodes.Count > 0) {
                    if ((node.inputNodes[0] as Factory).theRecipe == recipeProto) {
                        node.inputNodes.Clear();
                        continue;
                    }
                }
                DeleteDestinationRecipe(node, recipeProto);
            }
        }


        /// <summary>
        /// 统计每分钟需要的原材料
        /// </summary>
        /// <returns></returns>
        public void StatisticsInputResult()
        {
            ansNeedInput.Clear();
            foreach (RootItem topRoad in rootItems) {
                foreach (ItemRoad itemInput in topRoad.inItems) {
                    if (!IsOutHave(itemInput.TheItem.ID)) {
                        if (!ansNeedInput.ContainsKey(itemInput.TheItem.ID)) {
                            ansNeedInput[itemInput.TheItem.ID] = itemInput.needNumPerMin;
                        }
                        else {
                            ansNeedInput[itemInput.TheItem.ID] += itemInput.needNumPerMin;
                        } 
                    }
                }
            }
        }
        public void StatisticsDownResult()
        {
            //==================初始化统计表
            ansFactory.Clear();
            ansPicker = 0;
            ansPower[0] = 0;
            ansPower[1] = 0;
            ansPower[2] = 0;
            //初始化比例
            //CalAllOutRatio();
            HashSet<int> calRatio = new HashSet<int>();

            List<RootItem> levelResult = SortLevel();
            //遍历所有产物
            for (int i = 0; i < levelResult.Count; i++) {
                RootItem topRoad = levelResult[i];
                foreach (ItemRoad topOut in topRoad.outItems.Keys) {
                    topOut.needNumPerMin = 0;
                    //计算作为模块的需求
                    //遍历对应原材料
                    bool isNeedTheItem = false;
                    for (int j = 0; j < i; j++) {
                        RootItem topRoad2 = levelResult[j];
                        foreach (ItemRoad topOut2 in topRoad2.inItems) {
                            if (topOut2.TheItem.ID == topOut.TheItem.ID) {
                                topOut.needNumPerMin += topOut2.needNumPerMin;
                                isNeedTheItem = true;
                            }
                        }
                    }
                    //计算比例
                    if (!calRatio.Contains(topOut.TheItem.ID)&& isNeedTheItem) {
                        calRatio.Add(topOut.TheItem.ID);
                        float sum = 0;
                        int sum2 = 0;
                        //求和
                        for (int j = i; j < levelResult.Count; j++) {
                            RootItem topRoad2 = levelResult[j];
                            foreach (ItemRoad topOut2 in topRoad2.outItems.Keys) {
                                if (topOut2.TheItem.ID == topOut.TheItem.ID) {
                                    sum += topRoad2.outItems[topOut2][1];
                                }
                            }
                        }
                        //归一化
                        for (int j = i; j < levelResult.Count; j++) {
                            RootItem topRoad2 = levelResult[j];
                            foreach (ItemRoad topOut2 in topRoad2.outItems.Keys) {
                                if (topOut2.TheItem.ID == topOut.TheItem.ID) {
                                    //向下取整，sum2只可能小于100
                                    topRoad2.outItems[topOut2][1] = (int)(topRoad2.outItems[topOut2][1] / sum * 100);
                                    sum2 += topRoad2.outItems[topOut2][1];
                                }
                            }
                        }
                        //优化误差
                        for (int j = i; j < levelResult.Count; j++) {
                            RootItem topRoad2 = levelResult[j];
                            foreach (ItemRoad topOut2 in topRoad2.outItems.Keys) {
                                if (topOut2.TheItem.ID == topOut.TheItem.ID) {
                                    //向下取整，sum2只可能小于100
                                    topRoad2.outItems[topOut2][1] += 100 - sum2;
                                    j = levelResult.Count + 1;
                                    break;
                                }
                            }
                        }
                    }
                    
                    topOut.needNumPerMin *= topRoad.outItems[topOut][1] / 100f;
                    //写入用户需求
                    topOut.needNumPerMin += topRoad.outItems[topOut][0];
                    //统计输入提供部分
                    if (!ansNeedInput.ContainsKey(topOut.TheItem.ID)) {
                        ansNeedInput[topOut.TheItem.ID] = -topOut.needNumPerMin;
                    }
                    else {
                        ansNeedInput[topOut.TheItem.ID] -= topOut.needNumPerMin;
                    }
                }
                //计算输入原料需求
                topRoad.StatisticsDownResult();
                //统计输入
                foreach (ItemRoad inputRoad in topRoad.inItems) {
                    if (!ansNeedInput.ContainsKey(inputRoad.TheItem.ID)) {
                        ansNeedInput[inputRoad.TheItem.ID] = inputRoad.needNumPerMin;
                    }
                    else {
                        ansNeedInput[inputRoad.TheItem.ID] += inputRoad.needNumPerMin;
                    }
                }
            }
            //===================================统计输入原材料表
            //StatisticsInputResult();
            //==============统计成本
            foreach (RootItem rootItem in rootItems) {
                foreach (int factorId in rootItem.ansFactory.Keys) {
                    if (!ansFactory.ContainsKey(factorId)) {
                        ansFactory[factorId] = rootItem.ansFactory[factorId];
                    }
                    else {
                        ansFactory[factorId] += rootItem.ansFactory[factorId];
                    }
                }
                ansPicker += rootItem.ansPicker;
                ansPower[0] += rootItem.ansPower[0];
                ansPower[1] += rootItem.ansPower[1];
                ansPower[2] += rootItem.ansPower[2];
            }
        }
        public void CalOutRatio(int itemId)
        {
            if (outSet.Contains(itemId)) {
                float sum = 0;
                int sum2 = 0;
                //求和
                foreach (RootItem rootOut in rootItems) {
                    foreach (ItemRoad itemRoad in rootOut.outItems.Keys) {
                        if (itemRoad.TheItem.ID == itemId) {
                            sum += rootOut.outItems[itemRoad][1];
                        }
                    }
                }
                //归一化
                foreach (RootItem rootOut in rootItems) {
                    foreach (ItemRoad itemRoad in rootOut.outItems.Keys) {
                        if (itemRoad.TheItem.ID == itemId) {
                            //向下取整，sum2只可能小于100
                            rootOut.outItems[itemRoad][1] = (int)(rootOut.outItems[itemRoad][1] / sum * 100);
                            sum2 += rootOut.outItems[itemRoad][1];
                        }
                    }
                }
                //优化误差
                foreach (RootItem rootOut in rootItems) {
                    foreach (ItemRoad itemRoad in rootOut.outItems.Keys) {
                        if (itemRoad.TheItem.ID == itemId) {
                            //向下取整，sum2只可能小于100
                            rootOut.outItems[itemRoad][1] += 100 - sum2;
                            return;
                        }
                    }
                }

            }
        }
        public void CalAllOutRatio()
        {
            foreach (int id in outSet) {
                CalOutRatio(id);
            }
        }
        public List<ItemRoad> GetAllOut()
        {
            List<ItemRoad> ansItems = new List<ItemRoad>();
            ForEachOut((x)=> {
                ansItems.Add(x);
            });
            return ansItems;
        }

        public int ForEachOut(Action<ItemRoad> action)
        {
            int cnt = 0;
            foreach (RootItem rootItem in rootItems) {
                foreach (ItemRoad itemRoad in rootItem.outItems.Keys) {
                    action(itemRoad);
                    cnt++;
                }
            }
            return cnt;
        }
    }
}
