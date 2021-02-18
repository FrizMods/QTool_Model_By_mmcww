using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Qtools
{
    public class UITree
    {
        //============================================================图形格式参数
        /// <summary>
        /// 格子间距 参数 2
        /// </summary>
        protected float line = 2;
        /// <summary>
        /// 格子大小
        /// </summary>
        protected float side = 40;
        /// <summary>
        /// UI每行显示最大格子数 参数 40 
        /// </summary>
        protected float max = 40;
        /// <summary>
        /// 边界距离 参数 10
        /// </summary>
        protected float leftAndRight = 10;
        /// <summary>
        /// 顶边距 参数 25
        /// </summary>
        private float topOfMap = 25;
        //===========================窗口参数
        private Rect windowRect;
        public Rect maxWindowRect;
        public Vector2 windowPosition = new Vector2(0, 0);
        private bool mouseLock = false;
        private Vector3 lastMousePosition;

        public Material lineMaterial;
        /// <summary>
        /// 辅助数组 索引为layer标号
        /// </summary>
        private float[] posRight;

        //===============================UI 事件
        public TreeNode nowActiveNode;
        public bool isClickAnyNode = false;

        public float[] PosRight { get => posRight; set => posRight = value; }
        public Rect WindowRect { get => windowRect; set {
                windowRect = value;
                if (maxWindowRect == null) {
                    maxWindowRect = new Rect(windowRect);
                }
                else {
                    maxWindowRect.x = windowRect.x;
                    maxWindowRect.y = windowRect.y
; }

            }
        }

        Action<TreeNode> uIPressEvent;

        public UITree()
        {
            uIPressEvent = (x) => {
                nowActiveNode = x;
                isClickAnyNode = true;
            };
        }








        /// <summary>
        /// 绘制一个树
        /// </summary>
        /// <param name="theNode">当前绘制的节点</param>
        /// <param name="minPosX">树最左叶节点最小横坐标位置</param>
        private void DrawTree(TreeNode theNode, float minPosX)
        {
            //后序遍历
            //当前图标最小横坐标位置
            float theMin = minPosX > PosRight[theNode.layer] ? minPosX : PosRight[theNode.layer];
            if (theNode.inputNodes != null && theNode.inputNodes.Count > 0) {
                //子树
                //判断相邻子树的相邻结点  
                if (PosRight[theNode.layer] > PosRight[theNode.layer + 1] + line) {
                    //近似平衡结点结构
                    float temp = (theNode.treeWidth - 1) * 0.5f * (side + line);
                    //估计
                    //if (theNode.treeWidth != theNode.inputNodes.Count) {
                    //    temp += temp*(theNode.treeWidth - theNode.inputNodes.Count) * 0.02f;
                    //}
                    //矫正子树的位置
                    minPosX = theMin - temp;//posRight[theNode.layer] - temp;
                }
                //判断副产物-位置矫正
                if (theNode.inputNodes[0].outputNodes.Count > 1) {//theNode.GetType().Name == "ItemRoad" &&
                    if (minPosX <= PosRight[theNode.layer]) {
                        minPosX = minPosX + (theNode.inputNodes[0].outputNodes.Count - 1) * 0.5f * (side + line);
                    }
                }
                DrawTree(theNode.inputNodes[0], minPosX);
                for (int i = 1; i < theNode.inputNodes.Count; i++) {
                    TreeNode node = theNode.inputNodes[i];
                    DrawTree(node, PosRight[theNode.layer + 1]);
                }
                //中间结点
                float ans = (theNode.inputNodes[theNode.inputNodes.Count - 1].posRect.x + theNode.inputNodes[0].posRect.x) / 2;
                //考虑副产物
                if (theNode.inputNodes[0].outputNodes.Count > 1) {//((theNode is ItemRoad)||(theNode is RootItem)) && 
                    ans -= (theNode.inputNodes[0].outputNodes.Count - 1) * 0.5f * (side + line);
                    PosRight[theNode.layer] = ans > PosRight[theNode.layer] ? ans : PosRight[theNode.layer];
                    foreach (TreeNode road in theNode.inputNodes[0].outputNodes) {
                        //近似 位置矫正
                        road.posRect.x = PosRight[theNode.layer];
                        PosRight[theNode.layer] = PosRight[theNode.layer] + side + line;
                        ConnectLine(theNode.inputNodes[0], road);
                    }
                }
                else {
                    //近似 位置矫正
                    ans = ans > PosRight[theNode.layer] ? ans : PosRight[theNode.layer];
                    theNode.posRect.x = ans;
                    PosRight[theNode.layer] = theNode.posRect.x + side + line;

                }
                if (theNode.outputNodes.Count != 0) {
                    ConnectLine(theNode, theNode.outputNodes[0]);
                }

            }
            else {
                //叶结点位置由父节点允许的最小横坐标和当前层允许的最小横坐标决定
                //判断副产物
                theNode.posRect.x = theMin;
                PosRight[theNode.layer] = theNode.posRect.x + side + line;
                //可能根节点就是叶节点
                if (theNode.outputNodes.Count > 0) {
                    ConnectLine(theNode, theNode.outputNodes[0]);
                }

            }
            //设置位置
            //绘制UI
            //绘制多产物
            if (theNode.inputNodes.Count > 0 && theNode.inputNodes[0].outputNodes.Count > 1) {//&& theNode.GetType().Name == "ItemRoad" 
                foreach (TreeNode road in theNode.inputNodes[0].outputNodes) {
                    //近似 位置矫正

                    road.posRect.y = theNode.layer * 2 * (side + line) + topOfMap;
                    road.posRect.height = side;
                    road.posRect.width = side;
                    if (road.UIDraw()) {
                        isClickAnyNode = true;
                        nowActiveNode = road;
                    }

                }
            }
            else {
                theNode.posRect.y = theNode.layer * 2 * (side + line) + topOfMap;
                theNode.posRect.height = side;
                theNode.posRect.width = side;
                if (theNode.UIDraw()) {
                    isClickAnyNode = true;
                    nowActiveNode = theNode;
                }
            }
            //更新辅助数组
            //posRight[theNode.layer] = theNode.posRect.x + side + line;
        }



        public void DrawAllTrees(TreeManage treeManage)
        {
            if (treeManage.rootItems.Count == 0) {
                return;
            }
            PosRight = new float[treeManage.rootItems.Max<RootItem>().treeHight];
            for (int i = 0; i < PosRight.Length; i++) {
                PosRight[i] = leftAndRight + windowRect.x;
            }
            windowPosition = GUI.BeginScrollView(windowRect, windowPosition, maxWindowRect);
            foreach (TreeNode node in treeManage.rootItems) {
                DrawTree(node, PosRight[node.layer]);
            }
            GUI.EndScrollView();
            if (nowActiveNode != null) {
                DrawBorder(nowActiveNode, Color.red);
                //==================根节点改配方特殊操作
                if (ItemRoad.isNeedChangeRecipe) {
                    ItemRoad.isNeedChangeRecipe = false;
                    //treeManage.ChangeRecipe(nowActiveNode);
                    nowActiveNode = treeManage.ChangeRecipe(nowActiveNode);
                    //isClickAnyNode = false;
                }
                if (TreeNode.isNeedToModule) {
                    TreeNode.isNeedToModule = false;
                    treeManage.ToModule(nowActiveNode);
                    nowActiveNode = null;
                    isClickAnyNode = false;
                }
            }
            //更新最大画布尺寸
            maxWindowRect.width = 0;
            maxWindowRect.height = ((PosRight.Length + 2) * (side + line)) * 2;
            foreach (int t in posRight) {
                if (t > maxWindowRect.width) {
                    maxWindowRect.width = t;
                }
            }
            maxWindowRect.width += (side + line) * 5 - leftAndRight * 2;
            maxWindowRect.width = windowRect.width > maxWindowRect.width ? windowRect.width : maxWindowRect.width;
            maxWindowRect.height = windowRect.height > maxWindowRect.height ? windowRect.height : maxWindowRect.height;



            if (windowRect.Contains(Input.mousePosition)) {

                //====响应鼠标滚轮,图标放大缩小
                max -= (int)Input.mouseScrollDelta.y;
                max = max < 15 ? 15 : max;
                max = max > 52 ? 52 : max;
                side = (windowRect.width - 2 * leftAndRight - (max - 1) * line) / max;
                TreeNode.uiButtonStyle.fontSize = (int)(side / 5);
                TreeNode.uiLabelStyle.fontSize = (int)(side / 5);


                //============右键拖动画布
                if (Input.GetMouseButtonDown(1) && !mouseLock) {
                    mouseLock = true;
                    lastMousePosition = Input.mousePosition;
                }
                if (Input.GetMouseButtonUp(1) && mouseLock) {
                    mouseLock = false;
                }
                if (Input.GetMouseButton(1) && mouseLock) {
                    Vector2 offset = Input.mousePosition - lastMousePosition;
                    offset.x = -offset.x;
                    windowPosition += offset;
                    lastMousePosition = Input.mousePosition;
                    //QTools.QTool.DebugLog($"now{windowPosition.x}/{windowPosition.y}");
                }
                windowPosition.x = windowPosition.x > maxWindowRect.width - windowRect.width ? maxWindowRect.width - windowRect.width : windowPosition.x;
                windowPosition.x = windowPosition.x < 0 ? 0 : windowPosition.x;
                windowPosition.y = windowPosition.y > maxWindowRect.height - windowRect.height ? maxWindowRect.height - windowRect.height : windowPosition.y;
                windowPosition.y = windowPosition.y < 0 ? 0 : windowPosition.y;
            }
        }
        /// <summary>
        /// theNode1连theNode2 下连上
        /// 【特性】只进行了x轴的可见性判断
        /// </summary>
        /// <param name="parentNode"></param>
        private void ConnectLine(TreeNode theNode1, TreeNode theNode2)
        {
            float x1, x2, y1, y2;
            //============坐标系变换
            y1 = theNode1.posRect.y;
            y2 = theNode2.posRect.y + side;
            x1 = theNode1.posRect.x + side / 2;
            x2 = theNode2.posRect.x + side / 2;

            //==============坐标变换
            x1 = windowRect.x + x1 - windowPosition.x;
            x2 = windowRect.x + x2 - windowPosition.x;
            y1 = windowRect.y + y1 - windowPosition.y;
            y2 = windowRect.y + y2 - windowPosition.y;
            //==============y轴翻转
            y1 = windowRect.height - y1;
            y2 = windowRect.height - y2;
            //==============可见区域限制
            x1 = windowRect.x + windowRect.width < x1 ? windowRect.x + windowRect.width : x1;
            x2 = windowRect.x + windowRect.width < x2 ? windowRect.x + windowRect.width : x2;
            GL.PushMatrix(); //保存当前Matirx
            lineMaterial.SetColor("_Color", Color.black);
            lineMaterial.SetPass(0); //刷新当前材质
            GL.LoadPixelMatrix();//设置pixelMatrix
            GL.Begin(GL.LINES);
            GL.Vertex3(x1, y1, 0);
            GL.Vertex3(x1, (y1 + y2) / 2, 0);
            GL.Vertex3(x1, (y1 + y2) / 2, 0);
            GL.Vertex3(x2, (y1 + y2) / 2, 0);
            GL.Vertex3(x2, (y1 + y2) / 2, 0);
            GL.Vertex3(x2, y2, 0);

            GL.End();
            GL.PopMatrix();//读取之前的Matrix
        }
        /// <summary>
        /// 【特性】只对右侧进行了可见性判断
        /// </summary>
        /// <param name="theNode"></param>
        /// <param name="color"></param>
        private void DrawBorder(TreeNode theNode, Color color)
        {
            Rect theRect = new Rect(theNode.posRect);
            //=============坐标变换
            theRect.x -= windowPosition.x;
            theRect.y -= windowPosition.y;

            Vector3 p1, p2, p3, p4;
            bool flag = true;
            float theSide = line;
            p1 = new Vector3(theRect.x - theSide, windowRect.height - (theRect.y - theSide), 0);
            if (p1.x > windowRect.x + windowRect.width) {
                return;
            }
            p2 = new Vector3(theRect.xMax + theSide, windowRect.height - (theRect.y - theSide), 0);
            p3 = new Vector3(theRect.xMax + theSide, windowRect.height - (theRect.yMax + theSide), 0);
            if (p2.x > windowRect.x + windowRect.width) {
                flag = false;
                p2.x = windowRect.width + windowRect.x;
                p3.x = p2.x;
            }
            p4 = new Vector3(theRect.x - theSide, windowRect.height - (theRect.yMax + theSide), 0);
            GL.PushMatrix(); //保存当前Matirx
            lineMaterial.SetColor("_Color", color);
            lineMaterial.SetPass(0); //刷新当前材质
            GL.LoadPixelMatrix();//设置pixelMatrix
            GL.Begin(GL.LINES);
            //===1
            GL.Vertex(p1);
            GL.Vertex(p2);
            //===2
            if (flag) {
                GL.Vertex(p2);
                GL.Vertex(p3);
            }
            //===3
            GL.Vertex(p3);
            GL.Vertex(p4);
            //===4
            GL.Vertex(p4);
            GL.Vertex(p1);

            GL.End();
            GL.PopMatrix();//读取之前的Matrix
        }
    }
    /// <summary>
    /// 算法 
    /// 记录算法-Start end
    /// 施加对象 GetRect和子级Layout
    /// </summary>
    public class MyGUILayout
    {
        private int topline = 20;
        private int leftAndRight = 10;
        private Rect nowParentRect;

        private int nowPositionY=0;
        private int nowPositionX=0;
        private int nowLayerHeight = 0;

        public MyGUILayout(Rect parentRect) 
        {
            SetParentRect(parentRect);
        }
        public MyGUILayout SetLeftAndRight(int leftAndRight)
        {
            this.leftAndRight = leftAndRight;
            return this;
        }
        public MyGUILayout SetTopLine(int topline)
        {
            this.topline = topline;
            return this;
        }

        public MyGUILayout SetParentRect(Rect parentRect) 
        {
            nowParentRect = parentRect;
            nowPositionY = (int)(topline+parentRect.y);
            nowPositionX = 0;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizeScale">相对于父范围宽度的比例</param>
        /// <returns></returns>
        public MyGUILayout MoveNextRow(float sizeScale,int yline=3) 
        {
            nowPositionX = leftAndRight;
            nowPositionY += nowLayerHeight + yline;
            nowLayerHeight = (int)((nowParentRect.width-2*leftAndRight) * sizeScale);
            return this;
        }
        public MyGUILayout Start(float sizeScale)
        {
            nowLayerHeight = (int)((nowParentRect.width - 2 * leftAndRight) * sizeScale);
            nowPositionY = (int)(topline + nowParentRect.y);
            nowPositionX = leftAndRight;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizeScale">相对于父范围宽度的比例 0=本行剩余全部</param>
        /// <param name="leftLine">左边距</param>
        /// <returns></returns>
        public Rect MoveNext(float sizeScale = 0,int leftLine=0)
        {
            nowPositionX += leftLine;
            float width = (sizeScale == 0) ? (nowParentRect.width - nowPositionX - leftAndRight - leftLine) : (sizeScale * (nowParentRect.width - 2 * leftAndRight));
            Rect ansRect = new Rect(nowPositionX,nowPositionY, width, nowLayerHeight);
            //QTools.QTool.DebugLog($"x:{nowPositionX},y:{nowPositionY},width:{width},heigh:{nowLayerHeight}");
            nowPositionX += (int)width;
            return ansRect;
        }
        public Rect GetUsedRect()
        {
            Rect ansRect = new Rect();
            ansRect.y = nowParentRect.y;
            ansRect.x = 0;
            if (nowPositionX > leftAndRight) {
                ansRect.height = nowPositionY + nowLayerHeight;

            }
            else {
                ansRect.height = nowPositionY;
            }
            ansRect.width = nowParentRect.width;
            return ansRect;
        }
        public Rect GetRemainingRect()
        {
            Rect ansRect = new Rect();
            if (nowPositionX > leftAndRight) {
                ansRect.y = nowPositionY + nowLayerHeight;

            }
            else {
                ansRect.y = nowPositionY;
            }
            ansRect.x = 0;
            ansRect.width = nowParentRect.width;
            ansRect.height = nowParentRect.height - ansRect.y;
            return ansRect;
        }

        public int GetSize(float sizeScale, int num = 0,int yline=3) {
            return (int)((sizeScale * (nowParentRect.width - 2 * leftAndRight) + yline) * num);
        }

    }

}
