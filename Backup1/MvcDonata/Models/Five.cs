//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Text;

//namespace MvcDonata.Models.Five
//{
//    public class Computer
//    {
//        public static readonly int EMPTY = 0;
//        public static readonly int WHITE = 1;
//        public static readonly int BLACK = 2;

//        #region 形势图
//        private static List<Situation> Situations = new List<Situation>
//        {
//            new Situation
//            {
//                Name="五子连珠",
//                Mode=new int[][]{new int[]{1,1,1,1,1}},
//                Value=new int[2]{6000000,5000000}
//            },
//            new Situation
//            {
//                Name="活四",
//                Mode=new int[][] {new int[]{0,1,1,1,1,0}},
//                Value=600000
//            },
//            new Situation
//            {
//                Name="冲四",
//                Mode=new int[][] { new int[]{2,1,1,1,1,0},new int[]{2,1,0,1,1,1},new int[]{2,1,1,0,1,1} },
//                Value=9999
//            },
//            new Situation
//            {
//                Name="活三",
//                Mode=new int[][]{ new int[]{0,1,1,1,0},new int[]{0,1,0,1,1,0},new int[]{0,1,0,1,0,1,0}},
//                Value=17
//            },
//            new Situation
//            {
//                Name="眠三",
//                Mode=new int[][]{new int[]{2,1,1,1,0},new int[]{2,1,0,1,1,0},new int[]{2,1,0,1,0,1}},
//                Value=5
//            },
//            new Situation
//            {
//                Name="活二",
//                Mode=new int[][] { new int[]{0,1,1,0},new int[]{0,1,0,1,0},new int[]{0,1,0,0,1,0} },
//                Value=4
//            },
//            new Situation
//            {
//                Name="眠二",
//                Mode= new int[][] { new int[]{2,1,1,0},new int[]{2,1,0,1,0} },
//                Value=3
//            },
//            new Situation
//            {
//                Name="活一",
//                Mode=new int[][] { new int[]{0,1,0} },
//                Value=2
//            },           
//            new Situation
//            {
//                Name="眠一",
//                Mode=new int[][] { new int[]{2,1,0} },
//                Value=1
//            }

            
//        };

//        #endregion

//        public static readonly int Size=15;

//        public static readonly List<Position> Directions =
//            new List<Position>
//            {
//                new Position{Row=1,Col=1},
//                new Position{Row=1,Col=0},
//                new Position{Row=0,Col=1},
//                new Position {Row=1,Col=-1}
//            };





//        public static Step Sim(IEnumerable<Step> steps,int count)
//        {
//            var table=new Table(steps);
//            table.CalcAllSituations();

//            var canditate = new List<Table>();

//            foreach (var pos in table.EmptyPosition)
//            {
//                var newTable = new Table(table);
//                newTable.AddStep(new Step { Pos = pos, Side = table.First });
//                canditate.Add(newTable);
//            }

//            var query = (from t in canditate
//                         orderby t.CalcTotalValue(table.First) descending
//                        select t).Take(4);
//             var result1=query.Select(t=>Step(t, count, table.First, table.Last));

//            var list=new List<Table>();
//            foreach(var ts in result1)
//            {
//                list.AddRange(ts);
//            }
//            var first = table.First;
//            var last = table.Last;
//            var result = list.OrderByDescending(i=>i.CalcTotalValue(first)).First();
//            return (result - table).First();

            

//        }

//        public static IEnumerable<Table> Step(Table t,int count,int first,int last)
//        {
//           var result=new List<Table>();
//           if(t.Winner!=Computer.EMPTY)
//           {
//               result.Add(t);
//               return result;
//           }
//           if(count==0)
//           {
//               result.Add(t);
//               return result;
//           }

//           var list = new List<Table>();
//           foreach(var pos in t.EmptyPosition)
//           {
//               var newTable=new Table(t);
//               newTable.AddStep(new Step{ Pos=pos, Side=t.First});
//               list.Add(newTable);
//           }
//            //筛选四个最有价值的空格走棋
//           foreach (var tb in list.OrderByDescending(i=>i.CalcTotalValue(first)).Take(4))
//           {
//               result.AddRange(Step(tb, count - 1, first, last));
//           }

//           return result.OrderByDescending(i => i.CalcTotalValue(first)).Take(4);
//        }



//    }

//    public class Position
//    {
//        public int Row{get;set;}
//        public int Col{get;set;}

//        public override string ToString()
//        {
//            return string.Format("({0},{1})", Row, Col);
//        }

//        public bool IsInner
//        {
//            get
//            {
//                return Col >= 0 && Col < Computer.Size && Row >= 0 && Row < Computer.Size;
//            }
//        }
//        public static Position operator+ (Position a,Position b)
//        {
//            return new Position
//            {
//                 Col=a.Col+b.Col,
//                 Row=a.Row+b.Row
//            };
//        }
//        public static Position operator -(Position a)
//        {
//            return new Position
//            {
//                Col=-a.Col,
//                Row=-a.Row
//            };
//        }
//        public static Position operator -(Position a, Position b)
//        {
//            return a + (-b);
//        }

//        public static bool operator ==(Position a, Position b)
//        {
//            return a.Row == b.Row && a.Col == b.Col;
//        }
//        public static bool operator !=(Position a, Position b)
//        {
//            return !(a == b);
//        }

//        /// <summary>
//        /// 计算两个位置相隔几步棋
//        /// </summary>
//        /// <param name="a"></param>
//        /// <param name="b"></param>
//        /// <returns></returns>
//        public static int operator *(Position a, Position b)
//        {
//            return Math.Max(Math.Abs(a.Row - b.Row), Math.Abs(a.Col - b.Col))+1;
//        }

//        /// <summary>
//        /// 计算两个位置是否在五子棋的一条线上
//        /// </summary>
//        /// <param name="a"></param>
//        /// <param name="b"></param>
//        /// <returns></returns>
//        public static bool operator /(Position a, Position b)
//        {
//            return a.Row==b.Row||a.Col==b.Col||Math.Abs(a.Row - b.Row) == Math.Abs(a.Col - b.Col);
//        }
         
//    }


//    public class Situation
//    {
//        public string Name { get; set; }
//        public int[][] Mode { get; set; }
//        public int[] Value { get; set; }        
//    }

//    public class Step
//    {
        

//        public Position Pos{get;set;}
//        private int Side{get;set;}

//        private Step PreviousStep;
//        private List<Step> nextSteps;
        



//        public Step(Step pre,int row,int col,int side)
//        {
//            Pos = new Position { Row = row, Col = col };
//            Side = side;
//            PreviousStep = pre;
//        }             


//        public override string ToString()
//        {
//            return string.Format("最后一步:{0}", nextSteps.Last());
//        }

//        public List<Position> EmptyPosition
//        {
//            get
//            {
//                var list=new List<Position>();
//                for(var row=0;row<Computer.Size;row++)
//                {
//                    for(var col=0;col<Computer.Size;col++)
//                    {
//                        if(this[row,col]==Computer.EMPTY)
//                            list.Add(new Position{Row=row,Col=col});
//                    }
//                }
//                return list;
//            }
//        }


//        /// <summary>
//        /// 加入一步棋,并且重新计算形势
//        /// </summary>
//        /// <param name="step"></param>
//        public void AddStep(Step step)
//        {
//            nextSteps.Add(step);

//            foreach (var s in nextSteps)
//            {
//                if (step.Pos / s.Pos) //只计算一条线上的棋子
//                {
//                    step.Value=CalcPositionSituation(s.Pos).Item1;
//                }
//            }
//        }

//        public void RemoveLastStep()
//        {
//            var last = nextSteps.Last();
//            nextSteps.Remove(last);

//            for (var i = 0; i < nextSteps.Count;i++ )
//            {
//                if (last.Pos / nextSteps[i].Pos) //只计算一条线上的棋子
//                {
//                    nextSteps[i].Value = CalcPositionSituation(nextSteps[i].Pos).Item1-i;
//                }
//            }
//        }

//        public int CalcTotalValue(int side)
//        {

//            return nextSteps.Where(s => s.Side == side).Sum(s => s.Value) -
//                nextSteps.Where(s => s.Side != side && s.Side != Computer.EMPTY).Sum(s => s.Value);

//        }

        
//        public void CalcAllSituations()
//        {
//            for (var i = 0; i < nextSteps.Count;i++ )
//            {
//                nextSteps[i].Value = CalcPositionSituation(nextSteps[i].Pos).Item1-i;
//            }
//        }
//        private Tuple<int,List<Situation>> CalcPositionSituation(Position pos)
//        {


//            var value=0;
//            var ss = new List<Situation>();
//            foreach(var dir in Computer.Directions)
//            {
//                var result = IsDead(pos, dir);
//                if (result.Item1)
//                {
//                    ss.Add(new Situation
//                    {
//                        Name = "死棋",
//                        PieceString = result.Item2
//                    });
//                }
//                else
//                {

//                    foreach (var s in Situations)
//                    {
//                        if (IsMode(result.Item2, s.Mode))
//                        {
//                            value += s.Value;

//                            ss.Add(
//                                new Situation
//                                {
//                                    Name = s.Name,
//                                    PieceString = result.Item2,
//                                     Value=s.Value                                     
//                                });

//                            break;
//                        }
//                    }
                    
//                }
//            }

//            return new Tuple<int, List<Situation>>(value, ss);
//        }

//        public object GetPositionInfo(int row, int col)
//        {
//            return
//            new
//                {
//                    posSituations = CalcPositionSituation(new Position { Row = row, Col = col }).Item2,
//                    whiteSituation = CalcTotalValue(Computer.WHITE),
//                    blackSituation = CalcTotalValue(Computer.BLACK)
//                };
//        }

//        private string ReverseString(string input)
//        {
//            var sb = new StringBuilder();
//            foreach (var c in input.Reverse())
//            {
//                sb.Append(c);
//            }
//            return sb.ToString();
//        }
//        private bool IsMode(string input,List<string> mode)
//        {
//            var rev=ReverseString(input);
//            for (var i = 0; i < mode.Count; i++)
//            {
//                if (input.Contains(mode[i]) || rev.Contains(mode[i]))
//                    return true;
//            }
//            return false;
//        }


//        private Position CalcBorder(int side, Position start,Position dir)
//        {
//            start = new Position { Row = start.Row, Col = start.Col };
//            var i = 0;
//            while (start.IsInner&&i<5) //最多循环5次,多余五次没有意义
//            {
//                var target=this[start];
//                if (target != side && target != Computer.EMPTY)
//                    break;
//                start = start + dir;
//                i++;
//            }
//            return start - dir ;
//        }

//        public Tuple<bool, string> IsDead(Position pos,Position dir)
//        {
//            var target = this[pos];

//            var pos1 = CalcBorder(target, pos, dir);
//            var pos2 = CalcBorder(target, pos, -dir);
//            var len = pos1 * pos2;
//            var dead=false;
//            if (len < 5)
//            {
//                dead=true;
//            }

//            var sb = new StringBuilder();
//            sb.Append('2');
//            for (var i = pos2; i != pos1+dir; i = i + dir)
//            {
//                sb.Append(GetPositionChar(target, i));
//            }
//            sb.Append('2');
//            return new Tuple<bool,string>(dead, sb.ToString());
            
//        }

//        public int this[Position pos]
//        {
//            get
//            {
//                var cur = this;
//                while (cur != null)
//                {
//                    if (pos == cur.Pos)
//                    {
//                        return cur.Side;
//                    }
//                }
//                return Computer.EMPTY;
//            }
//        }
//        public int this[int row, int col]
//        {
//            get
//            {
//                return this[new Position { Row = row, Col = col }];
//            }
//        }

//        public int First
//        {
//            get
//            {
//                return Side;
//            }
//        }
//        public int Last
//        {
//            get
//            {
//                return Side == Computer.BLACK ? Computer.WHITE : Computer.BLACK;
//            }
//        }
       
//    }
//}