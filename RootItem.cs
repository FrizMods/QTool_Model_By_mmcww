using System;
using System.Collections.Generic;

namespace Qtools
{
    public class RootItem : ItemRoad, IComparable<RootItem>
    {
        /// <summary>
        /// 输出表 0-数量 1-比例
        /// </summary>
        public Dictionary<ItemRoad,int[]> outItems = new Dictionary<ItemRoad, int[]>();
        //输入表
        public List<ItemRoad> inItems = new List<ItemRoad>();
        //配方排除表 用于树更新
        public HashSet<int> inDebar;

        //===========统计
        public Dictionary<int, float> ansNeedInput;
        /// <summary>
        /// [id] = 数量
        /// </summary>
        public Dictionary<int, int> ansFactory;
        /// <summary>
        /// [0]最大功耗 [1]额定功耗 [2]待机功耗
        /// </summary>
        public long[] ansPower;
        /// <summary>
        /// 爪子数量统计 三种分拣器id 2011 2012 2013
        /// </summary>
        public int ansPicker = 0;
        public RootItem(int itemId) : base(itemId)
        {
            
        }

        private void StatisticsD(TreeNode x)
        {
            if (x is Factory) {
                Factory factory = (Factory)x;
                //==============统计功耗
                ItemProto factorItem = LDB.items.Select(factory.FactoryId);
                //RootItem rootItem = (RootItem)myRootNode;
                ansPower[0] += (long)((factorItem.prefabDesc.workEnergyPerTick)
                                          * factory.Num * 60L);
                ansPower[1] += (long)((factorItem.prefabDesc.workEnergyPerTick * factory.Efficiency + (1 - factory.Efficiency) * factorItem.prefabDesc.idleEnergyPerTick)
                                          * factory.Num * 60L);
                ansPower[2] += (long)((factorItem.prefabDesc.idleEnergyPerTick)
                                          * factory.Num * 60L);
                //=============================统计爪子数量
                ansPicker += inputNodes.Count + outputNodes.Count;
                //=============================统计工厂数量
                if (!ansFactory.ContainsKey(factory.FactoryId)) {
                    ansFactory[factory.FactoryId] = factory.Num;
                }
                else {
                    ansFactory[factory.FactoryId] += factory.Num;
                }
            }
            else {
                ItemRoad itemRoad = (ItemRoad)x;
                if (!ansNeedInput.ContainsKey(itemRoad.TheItem.ID)) {
                    ansNeedInput[itemRoad.TheItem.ID] = itemRoad.needNumPerMin;
                }
                else {
                    ansNeedInput[itemRoad.TheItem.ID] += itemRoad.needNumPerMin;
                }
            }
        }

        public override void StatisticsDownResult()
        {
            //初始化
            ansPower = new long[3] { 0, 0, 0 };
            ansFactory = new Dictionary<int, int>();
            ansNeedInput = new Dictionary<int, float>();
            Statistics = StatisticsD;
            //开始统计
            base.StatisticsDownResult();
        }

        public RootItem(ItemRoad road) : base(road)
        {

        }

        public ItemRoad FindOut(int itemId)
        {
            foreach (ItemRoad itemRoad in outItems.Keys) {
                if (itemRoad.TheItem.ID == itemId) {
                    return itemRoad;
                }
            }
            return null;
        }

        public RootItem SetInDebar(HashSet<int> inDebar)
        {
            this.inDebar = inDebar;
            return this;
        }


        public override void UpdateTree()
        {
            LeafUpdateCallback = (x) =>
            {
                inItems.Add(x);
            };
            MultiUpdateCallback = (x) =>
            {
                //不存在x和this相同的情况
                outItems[x] = new int[2] { 0, 100 };
            };

            inItems.Clear();
            Dictionary<ItemRoad, int[]> temp = new Dictionary<ItemRoad, int[]>(outItems);
            outItems.Clear();
            myRootNode = this;
            outItems[this] = new int[2] { 0, 100 };
            layer = 0;
            base.UpdateTree();
            //===========还原用户设置的数据
            foreach (ItemRoad itemRoad in outItems.Keys) {
                if (temp.ContainsKey(itemRoad)) {
                    outItems[itemRoad][0] = temp[itemRoad][0];
                    outItems[itemRoad][1] = temp[itemRoad][1];
                }
            }
        }

        public int CompareTo(RootItem other)
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




}
