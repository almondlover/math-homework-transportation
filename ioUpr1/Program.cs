using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace ioUpr1
{
	internal class Program
	{
		static bool isValidPath(int[][] seq)
		{
			if (seq.GetLength(0)<4||seq.GetLength(0) % 2!=0)
				return false;
			for (int i=1; i<seq.Length; i++)
			{
				if (seq[i - 1][ (i + 1) % 2] != seq[i][ (i + 1) % 2])
					return false;
			}
			if (seq[seq.GetLength(0) - 1][0] != seq[0][0]
				&& seq[seq.GetLength(0) - 1][1] != seq[0][1])
				return false;
			return true;
		}
		static void extendPath(List<int[]> path, int[][] list, List<List<int[]>> paths, int maxDepth)
		{
			if (isValidPath(path.ToArray()))
			{
				paths.Add(path);
				return;
			}
			if (maxDepth>-100)
			{
				foreach(int[] cell in list)
				{
					//sashtiq stalb/red
					if (path[path.Count - 1][(path.Count + 1) % 2] == cell[1 + (path.Count + 1) % 2] && !path.Any(e=>e.SequenceEqual(new int[] { cell[1], cell[2] })))
					{
						var newPath = new List<int[]>();
						newPath.AddRange(path);
                        newPath.Add(new int[] { cell[1], cell[2] });
						extendPath(newPath, list, paths, --maxDepth); 
					}
				}
			}
		}
		static List<List<int[]>> findAllCycles(int[][] list1, int[][] list2, int depth)
		{
			var paths=new List<List<int[]>>();
			foreach (int[] cell in list2)
			{
				extendPath(new List<int[]> { new int[] { cell[1], cell[2] } }, list1, paths, depth);
			}
			return paths;
		}
		static void northWestCornerMethod(int[] supply, int[] demand, int[,] matrix, bool[,] marked, int row, int col, List<int[]> list1, List<int[]> list2)
		{
			//init empty
			for (int i = 0; i < matrix.GetLength(0); i++)
				for (int j = 0; j < matrix.GetLength(1); j++)
					list2.Add(new int[] { matrix[i, j], i, j });

			while (row<matrix.GetLength(0)&&col<matrix.GetLength(1))
			{
				var lower = Math.Min(supply[row], demand[col]);
				matrix[row, col] = lower;
				marked[row, col] = true;
				//tuka posle trq go opraq
				list2.RemoveAll(arr=>arr.SequenceEqual(new int[] { 0, row, col }));
				list1.Add(new int[] { matrix[row, col], row, col });

				//end hui
				supply[row]-= lower;
				demand[col]-= lower;
				if (supply[row] == 0&&demand[col]!=0) row++;
				else col++;
			}
		}
		static void minimalElement(int[] supply, int[] demand, int[,] matrix, bool[,] marked, int emptyrow, int emptycol, int[,] cmatrix, List<int[]> list1, List<int[]> list2)
		{
            //init empty
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    list2.Add(new int[] { matrix[i, j], i, j });

			int[] minel = new int[3];
			
			while (supply.Sum()>0&&demand.Sum()>0)
			{
                minel[0] = int.MaxValue;

				int zeroedcol = demand.Count((e)=>e==0);
				//find minimal cost 
				foreach(var e in list2)
				{
					int i = e[1], j=e[2];
					if (supply[i] == 0) continue;
					if (demand[j] == 0) {
						if (zeroedcol<emptycol) continue;
						zeroedcol++;
					}
                    if (minel[0] > cmatrix[i, j]) minel = new int[] { cmatrix[i, j], i, j };
                }
    //            for (int i = 0; i < matrix.GetLength(0); i++)
				//{
				//	if (supply[i] == 0) continue;
				//	int j, empty=0;
				//	for (j = 0; j < matrix.GetLength(1); j++)
				//	{
				//		//dont skip if base 0
				//		if (demand[j] == 0&&empty++<emptycol) continue;
				//		//y save value tho??????
				//		if (minel[0] > cmatrix[i, j]) minel = new int[] { cmatrix[i, j], i, j};
				//	}
				//	//if (minel[0] > cmatrix[i, j]) minel = new int[] { cmatrix[i, j], i, j };
    //            }
                var lower = Math.Min(supply[minel[1]], demand[minel[2]]);
                matrix[minel[1], minel[2]] = lower;
                
                list2.RemoveAll(arr => arr.SequenceEqual(new int[] { 0, minel[1], minel[2] }));
                list1.Add(new int[] { lower, minel[1], minel[2] });

                //end hui
                supply[minel[1]] -= lower;
                demand[minel[2]] -= lower;
                if (demand[minel[2]] == 0 && supply[minel[1]] != 0) emptycol++;
                //else emptyrow++;

            }

		}
		static void potentialMethod(int[,] matrix, /*bool[,] marked,*/ List<int[]> list1, List<int[]> list2, int[,] cmatrix, int depth)
		{
			int maxd = 0;
			do
			{
				var loop = findAllCycles(list1.ToArray(), list2.ToArray(), depth);
				var u = new List<int?>(new int?[matrix.GetLength(0)]);
				var v = new List<int?>(new int?[matrix.GetLength(1)]);
				u[0] = 0;

				//🙏dano bachka
				solveSystem(cmatrix, list1, u, v, 0, -1);
				//bachka💪💪💪💪

				for (int i = 0; i < v.Count; i++)
					Console.Write(" " + v[i]);
				Console.WriteLine();

				for (int i = 0; i < u.Count; i++)
					Console.Write(u[i] + "\n");
				Console.WriteLine();

				List<int> delta = new List<int>();
				int cur;
				int[] minx = new int[3];
				minx[0] = int.MaxValue;


				foreach (int[] cell in list2)
				{
					cur = (int)(u[cell[1]] + v[cell[2]]) - cmatrix[cell[1], cell[2]];

					delta.Add(cur);
				}
				maxd = delta[0];
				//worst delta
				foreach (int d in delta)
					if (maxd < d) maxd = d;

				Console.WriteLine(maxd);
				if (maxd <= 0) break;

				//get min (-) in loop
				int[][] maxdLoop = loop[delta.IndexOf(maxd)].ToArray();
				for (int i = 1; i < maxdLoop.Length; i += 2)
					if (minx[0] > matrix[maxdLoop[i][0], maxdLoop[i][1]])
						minx = new int[] { matrix[maxdLoop[i][0], maxdLoop[i][1]], maxdLoop[i][0], maxdLoop[i][1] };

				//move value from full to empty
				list1.Remove(list1.First(arr => arr[1] == minx[1] && arr[2] == minx[2]));
                list1.Add(new int[] { minx[0], maxdLoop[0][0], maxdLoop[0][1] });
                list2.Remove(list2.First(arr => arr.SequenceEqual(new int[] { 0, maxdLoop[0][0], maxdLoop[0][1] })));
                list2.Add(new int[]{ 0, minx[1], minx[2]});

				for (int i = 0; i < maxdLoop.Length; i += 2)
					matrix[maxdLoop[i][0], maxdLoop[i][1]] += minx[0];
				for (int i = 1; i < maxdLoop.Length; i += 2)
					matrix[maxdLoop[i][0], maxdLoop[i][1]] -= minx[0];

				//print
				int f = 0;
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        Console.Write(matrix[i, j] + " ");
						f += matrix[i, j] * cmatrix[i, j];
                    }
                    Console.WriteLine();
                }
				Console.WriteLine($"f={f}\nend iter.\n");
            }
			while (maxd > 0);
        }
		static void solveSystem(int[,] cmatrix, List<int[]> list, List<int?> x, List<int?> y, int i, int j)
		{
			foreach (int[] cell in list)
			{
				if (cell[1] == i && y[cell[2]] == null)
				{
					y[cell[2]] = cmatrix[i, cell[2]] - x[i];
					solveSystem(cmatrix, list, x, y, -1, cell[2]);
				}
				else if (cell[2] == j && x[cell[1]] == null)
				{
					x[cell[1]] = cmatrix[cell[1], j] - y[j];
					solveSystem(cmatrix, list, x, y, cell[1], -1);
				}
			}
		}
		static void distributionMethod(int[,] matrix, List<int[]> list1, List<int[]> list2, int[,] cmatrix, int depth)
		{
			int maxd = 0;
			do
            {
                var loop = findAllCycles(list1.ToArray(), list2.ToArray(), depth);

                List<int> delta = new List<int>();

				int sumc;
                int cursum=0;
                int[] minx = new int[3];
                minx[0] = int.MaxValue;

				foreach (var curloop in loop)
				{
					for (int i = 0; i < curloop.Count; i += 2)
						cursum -= cmatrix[curloop[i][0],curloop[i][1]];
                    for (int i = 1; i < curloop.Count; i += 2)
                        cursum += cmatrix[curloop[i][0], curloop[i][1]];
					delta.Add(cursum);
					cursum = 0;
                }

                maxd = delta[0];
                //worst delta
                foreach (int d in delta)
                    if (maxd < d) maxd = d;

                Console.WriteLine(maxd);
                if (maxd <= 0) break;

                //get min (-) in loop
                int[][] maxdLoop = loop[delta.IndexOf(maxd)].ToArray();
                for (int i = 1; i < maxdLoop.Length; i += 2)
                    if (minx[0] > matrix[maxdLoop[i][0], maxdLoop[i][1]])
                        minx = new int[] { matrix[maxdLoop[i][0], maxdLoop[i][1]], maxdLoop[i][0], maxdLoop[i][1] };

                //move value from full to empty
                list1.Remove(list1.First(arr => arr[1] == minx[1] && arr[2] == minx[2]));
                list1.Add(new int[] { minx[0], maxdLoop[0][0], maxdLoop[0][1] });
                list2.Remove(list2.First(arr => arr.SequenceEqual(new int[] { 0, maxdLoop[0][0], maxdLoop[0][1] })));
                list2.Add(new int[] { 0, minx[1], minx[2] });

                for (int i = 0; i < maxdLoop.Length; i += 2)
                    matrix[maxdLoop[i][0], maxdLoop[i][1]] += minx[0];
                for (int i = 1; i < maxdLoop.Length; i += 2)
                    matrix[maxdLoop[i][0], maxdLoop[i][1]] -= minx[0];

                //print
                int f = 0;
                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        Console.Write(matrix[i, j] + " ");
                        f += matrix[i, j] * cmatrix[i, j];
                    }
                    Console.WriteLine();
                }
                Console.WriteLine($"f={f}\nend iter.\n");
            }
            while (maxd > 0);
        }
		static void Main(string[] args)
		{
			var supply = new int[] { 110, 70, 40 };
			var demand = new int[] { 110, 60, 50 };
			var matrix = new int[3, 3];
			var cmatrix = new int[,] { { 4, 6, 3},
										{ 1, 3, 2},
										{ 0, 0, 0 } };
			var marked = new bool[3, 4];
			var full = new List<int[]>();
			var empty = new List<int[]>();


			minimalElement(supply, demand, matrix, marked, 0, 0, cmatrix, full, empty);

			potentialMethod(matrix, full, empty, cmatrix, 5);

			for (int i=0; i<matrix.GetLength(0); i++)
			{
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					Console.Write(matrix[i,j] + " ");
				}
				Console.WriteLine();
			}
			for (int i = 0; i < matrix.GetLength(0); i++)
			{
				for (int j = 0; j < matrix.GetLength(1); j++)
				{
					Console.Write(marked[i,j] + " ");
				}
				Console.WriteLine();
			}
		}
	}
}
