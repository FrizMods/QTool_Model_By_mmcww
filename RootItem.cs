using  System ;
using  System . Collections . Generic ;

namespace  Qtools
{
    public  class  RootItem : ItemRoad , IComparable < RootItem >
    {
        /// < summary >
        /// Output table 0-quantity 1-proportion
        /// </ summary >
        public  Dictionary < ItemRoad , int []> outItems  =  new  Dictionary < ItemRoad , int []>();
        // Input table
        public  List < ItemRoad > inItems  =  new  List < ItemRoad >();
        // Recipe exclusion table is used for tree update
        public  HashSet < int > inSet , outSet ;

        public  int  level  =  0 ;

        // =========== Statistics
        public  Dictionary < int , float > ansProductTimes ;

        public  Dictionary < int , float > ansNeedInput ;
        /// < summary >
        /// [id] = quantity
        /// </ summary >
        public  Dictionary < int , int > ansFactory ;
        /// < summary >
        /// [0] Maximum power consumption [1] Rated power consumption [2] Standby power consumption
        /// </ summary >
        public  long [] ansPower ;
        /// < summary >
        /// Statistics of the number of claws, three sorters id 2011 2012 2013
        /// </ summary >
        public  int  ansPicker  =  0 ;
        public  RootItem ( int  itemId ): base ( itemId )
        {
            
        }

        private  void  StatisticsD ( TreeNode  x )
        {
            if ( x  is  Factory ) {
                Factory  factory  = ( Factory ) x ;
                // ==============Statistical power consumption
                ItemProto  factorItem  =  LDB . Items . The Select ( Factory . FactoryId );
                // RootItem rootItem = (RootItem)myRootNode;
                ansPower [ 0 ] += ( long )(( factorItem . prefabDesc . workEnergyPerTick )
                                          *  factory . Num  *  60 L );
                ansPower [ 1 ] += ( long )(( factorItem . prefabDesc . workEnergyPerTick  *  factory . Efficiency  + ( 1  -  factory . Efficiency ) *  factorItem . prefabDesc . idleEnergyPerTick )
                                          *  factory . Num  *  60 L );
                ansPower [ 2 ] += ( long )(( factorItem . prefabDesc . idleEnergyPerTick )
                                          *  factory . Num  *  60 L );
                // ============================= count the number of claws
                ansPicker  += ( factory . inputNodes . Count  +  factory . outputNodes . Count ) *  factory . Num ;
                // ============================= count the number of factories
                if ( ! ansFactory . ContainsKey ( factory . FactoryId )) {
                    ansFactory [ factory . FactoryId ] =  factory . Num ;
                }
                else {
                    ansFactory [ factory . FactoryId ] +=  factory . Num ;
                }
            }
            else {
                ItemRoad  itemRoad  = ( ItemRoad ) x ;
                if ( ! ansNeedInput . ContainsKey ( itemRoad . TheItem . ID )) {
                    ansNeedInput [ itemRoad . TheItem . ID ] =  itemRoad . needNumPerMin ;
                }
                else {
                    ansNeedInput [ itemRoad . TheItem . ID ] +=  itemRoad . needNumPerMin ;
                }
            }
        }

        public  override  void  StatisticsDownResult ()
        {
            // Initialization
            ansPower  =  new  long [ 3 ] { 0 , 0 , 0 };
            ansFactory  =  new  Dictionary < int , int >();
            ansNeedInput  =  new  Dictionary < int , float >();
            Statistics  =  StatisticsD ;
            // Start statistics
            base . StatisticsDownResult ();
        }

        public  RootItem ( ItemRoad  road ): base ( road )
        {

        }

        public  ItemRoad  FindOut ( int  itemId )
        {
            foreach ( ItemRoad  itemRoad  in  outItems . Keys ) {
                if ( itemRoad . TheItem . ID  ==  itemId ) {
                    return  itemRoad ;
                }
            }
            return  null ;
        }

        public  RootItem  SetInAndOut ( HashSet < int > inSet , HashSet < int > outSet )
        {
            this . inSet  =  inSet ;
            this . outSet  =  outSet ;
            return  this ;
        }


        public  override  void  UpdateTree ( int  parentItemId )
        {
            LeafUpdateCallback  = ( x ) =>
            {
                inItems . Add ( x );
            };
            MultiUpdateCallback  = ( x ) =>
            {
                //There is no case where x and this are the same
                outItems [ x ] =  new  int [ 2 ] { 0 , 100 };
            };
            ansProductTimes  =  new  Dictionary < int , float >();
            inItems . Clear ();
            Dictionary < ItemRoad , int []> temp  =  new  Dictionary < ItemRoad , int []>( outItems );
            outItems . Clear ();
            productTimes  =  1 f ;
            myRootNode  =  this ;
            outItems [ this ] =  new  int [ 2 ] { 0 , 100 };
            layer  =  0 ;
            base . UpdateTree ( theItem . ID );
            // =========== Restore the data set by the user
            foreach ( ItemRoad  itemRoad  in  outItems . Keys ) {
                if ( temp . ContainsKey ( itemRoad )) {
                    outItems [ itemRoad ][ 0 ] =  temp [ itemRoad ][ 0 ];
                    outItems [ itemRoad ][ 1 ] =  temp [ itemRoad ][ 1 ];
                }
            }
            // ===========Comprehensive output and input magnification, establish a comprehensive table
            foreach ( ItemRoad  inputRoad  in  inItems ) {
                if ( ! ansProductTimes . ContainsKey ( inputRoad . TheItem . ID )) {
                    ansProductTimes [ inputRoad . theItem . ID ] =  - inputRoad . productTimes ;
                }
                else {
                    ansProductTimes [ inputRoad . TheItem . ID ] -=  inputRoad . productTimes ;
                }
            }
            foreach ( ItemRoad  outputRoad  in  outItems . Keys ) {
                if ( ! ansProductTimes . ContainsKey ( outputRoad . TheItem . ID )) {
                    ansProductTimes [ outputRoad . TheItem . ID ] =  outputRoad . productTimes ;
                }
                else {
                    ansProductTimes [ outputRoad . TheItem . ID ] +=  outputRoad . productTimes ;
                }
            }


        }

        public  int  CompareTo ( RootItem  other )
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




}