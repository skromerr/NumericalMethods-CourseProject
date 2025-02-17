using System.Linq.Expressions;
using System.Transactions;

namespace courseProject;

public static class Const 
{
   public const double max = 1e+60;
}

public class FEM
{
   private Grid grid;
   private SparseMatrix globalMatrix = default!;
   private Vector globalVector = default!;
   private SLAE slae = default!;
   private Matrix alphas;
   private Vector localVector;
   private Matrix stiffnessMatrix;
   private Matrix massMatrix;
   private PointRZ[] vertices;
   private FirstCondition[] firstConditions;
   private SecondCondition[] secondConditions;

   public FEM(Grid grid)
   {
      this.grid = grid;
      alphas = new(3);
      stiffnessMatrix = new(6);
      massMatrix = new(6);
      localVector = new(6);

      vertices = new PointRZ[3];

      using (var sr = new StreamReader("C:/Users/Skromer/source/repos/courseProject/courseProject/firstConditions.txt"))
      {
         int count = Convert.ToInt32(sr.ReadLine());

         firstConditions = new FirstCondition[count];

         string[] data;

         for (int i = 0; i < count; i++)
         {
            data = sr.ReadLine()!.Split(" ").ToArray();
            firstConditions[i] = new(grid.Node[int.Parse(data[0])], int.Parse(data[0]), int.Parse(data[1]));
         }
      }

      using (var sr = new StreamReader("C:/Users/Skromer/source/repos/courseProject/courseProject/secondConditions.txt"))
      {
         int count = Convert.ToInt32(sr.ReadLine());

         secondConditions = new SecondCondition[count];

         string[] data;

         for (int i = 0; i < count; i++)
         {
            data = sr.ReadLine()!.Split(" ").ToArray();
            secondConditions[i] = new(int.Parse(data[0]), int.Parse(data[1]), int.Parse(data[2]));
         }
      }
   }

   private double func(PointRZ point, int ielem) => grid.Gamma * U(point) - grid.Lambda[ielem] * 1 / point.R;

   public double U(PointRZ point) => point.R;

   public void Compute()
   {
      BuildPortrait();
      AssemblyGlobalMatrix();
      AccountSecondConditions();
      AccountFirstConditions();
      slae = new SLAE(1000, 1e-60, globalVector, globalMatrix);
      slae.CGM();
   }
   public void ComputeWithMax()
   {
      BuildPortrait();
      AssemblyGlobalMatrix();
      AccountSecondConditions();
      AccountFirstConditionsWithMax();
      slae = new SLAE(1000, 1e-100, globalVector, globalMatrix);
      slae.CGMWithMax();
   }

   public void PrintSolution()
   {
      Console.WriteLine("Численное решение");
      slae.PrintSolution();
      Vector exactSolution = new(grid.Node.Length);
      Console.WriteLine("Точное решение");
      for (int i = 0; i < exactSolution.Length; i++)
      {
         exactSolution[i] = U(grid.Node[i]);
         Console.WriteLine($"{exactSolution[i]}");
      }
      Console.WriteLine("Погрешность");
      Vector inaccuracy = slae.solution - exactSolution;
      for (int i = 0; i < inaccuracy.Length; i++)
      {
         Console.WriteLine($"{inaccuracy[i]}");
      }
      Console.WriteLine("Норма погрешности");
      Console.WriteLine($"{inaccuracy.Norm()}");
   }

   public void AccountFirstConditions()
   {
      foreach (var fc in firstConditions)
      {
         globalMatrix.Di[fc.NodeNumber] = 1;
         globalVector[fc.NodeNumber] = fc.Ug();
         for (int i = globalMatrix.Ig[fc.NodeNumber]; i < globalMatrix.Ig[fc.NodeNumber + 1]; i++)
         {
            globalVector[globalMatrix.Jg[i]] -= fc.Ug() * globalMatrix.Gg[i];
            globalMatrix.Gg[i] = 0;
         }
         for (int i = fc.NodeNumber + 1; i < globalMatrix.Size; i++)
         {
            for (int j = globalMatrix.Ig[i]; j < globalMatrix.Ig[i + 1]; j++)
            {
               if (globalMatrix.Jg[j] == fc.NodeNumber)
               {
                  globalVector[i] -= fc.Ug() * globalMatrix.Gg[j];
                  globalMatrix.Gg[j] = 0;
               }
            }
         }
      }
   }

   public void AccountFirstConditionsWithMax()
   {
      foreach (var fc in firstConditions)
      {
         globalMatrix.Di[fc.NodeNumber] = Const.max;
         globalVector[fc.NodeNumber] = fc.Ug() * Const.max;
      }
   }

   public void AccountSecondConditions()
   {
      int[] edgeNodes = new int[3];

      foreach (var sc in secondConditions)
      {
         localVector.Fill(0);
         int ielem = sc.ElemNumber;

         vertices[0] = grid.Node[grid.Elements[ielem][0]];
         vertices[1] = grid.Node[grid.Elements[ielem][1]];
         vertices[2] = grid.Node[grid.Elements[ielem][2]];
         CalcuclateAlphas();

         if ((ielem + sc.EdgeType) % 2 == 0)
         {
            edgeNodes[0] = 0;
            edgeNodes[1] = 3;
            edgeNodes[2] = 1;
         }
         else
         {
            edgeNodes[0] = 1;
            edgeNodes[1] = 4;
            edgeNodes[2] = 2;
         }

         PointRZ start = new(grid.Node[grid.Elements[ielem][edgeNodes[0]]].R, grid.Node[grid.Elements[ielem][edgeNodes[0]]].Z);
         PointRZ end = new(grid.Node[grid.Elements[ielem][edgeNodes[2]]].R, grid.Node[grid.Elements[ielem][edgeNodes[2]]].Z);

         for (int i = 0; i < 3; i++)
         {
            localVector[edgeNodes[i]] = grid.Lambda[ielem] * GaussEdge(edgeNodes[i], sc.Theta, sc.FuncNumber, start, end);
         }

         AddElementToVector(ielem);
      }
   }

   //public void BuildPortrait()
   //{
   //   List<int>[] list = new List<int>[grid.Node.Length].Select(_ => new List<int>()).ToArray();
   //   foreach (var element in grid.Elements.Select(array => array.OrderBy(value => value).ToArray()).ToArray())
   //   {
   //      for (int i = 0; i < element.Length; i++)
   //      {
   //         for (int j = i + 1; j < element.Length; j++)
   //         {
   //            int pos = element[j];
   //            int elem = element[i];

   //            if (!list[pos].Contains(elem))
   //            {
   //               list[pos].Add(elem);
   //            }
   //         }
   //      }
   //   }

   //   list = list.Select(list => list.OrderBy(value => value).ToList()).ToArray();
   //   int count = list.Sum(childList => childList.Count);

   //   globalMatrix = new(grid.Node.Length, count);
   //   globalVector = new(grid.Node.Length);

   //   globalMatrix.Ig[0] = 0;

   //   for (int i = 0; i < list.Length; i++)
   //      globalMatrix.Ig[i + 1] = globalMatrix.Ig[i] + list[i].Count;

   //   int k = 0;

   //   foreach (var childList in list)
   //   {
   //      foreach (var value in childList)
   //      {
   //         globalMatrix.Jg[k] = value;
   //         k++;
   //      }
   //   }
   //}

   public void BuildPortrait()
   {
      HashSet<int>[] list = new HashSet<int>[grid.Node.Length].Select(_ => new HashSet<int>()).ToArray();
      foreach (var element in grid.Elements)
      {
         foreach (var pos in element)
         {
            foreach (var node in element)
            {
               if (pos > node)
               {
                  list[pos].Add(node);
               }
            }
         }
      }

      list = list.Select(childlist => childlist.OrderBy(value => value).ToHashSet()).ToArray();
      int count = list.Sum(childList => childList.Count);

      globalMatrix = new(grid.Node.Length, count);
      globalVector = new(grid.Node.Length);

      globalMatrix.Ig[0] = 0;

      for (int i = 0; i < list.Length; i++)
         globalMatrix.Ig[i + 1] = globalMatrix.Ig[i] + list[i].Count;

      int k = 0;

      foreach (var childList in list)
      {
         foreach (var value in childList)
         {
            globalMatrix.Jg[k++] = value;
         }
      }
   }

   private void AddElement(int i, int j, double value)
   {
      if (i == j)
      {
         globalMatrix.Di[i] += value;
         return;
      }

      for (int icol = globalMatrix.Ig[i]; icol < globalMatrix.Ig[i + 1]; icol++)
      {
         if (globalMatrix.Jg[icol] == j)
         {
            globalMatrix.Gg[icol] += value;
            return;
         }
      }
   }

   private void AssemblyGlobalMatrix()
   {
      for (int ielem = 0; ielem < grid.Elements.Length; ielem++)
      {
         vertices[0] = grid.Node[grid.Elements[ielem][0]];
         vertices[1] = grid.Node[grid.Elements[ielem][1]];
         vertices[2] = grid.Node[grid.Elements[ielem][2]];

         AssemblyLocalMatrixes();

         stiffnessMatrix = grid.Lambda[ielem] * stiffnessMatrix + grid.Gamma * massMatrix;

         for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
               AddElement(grid.Elements[ielem][i], grid.Elements[ielem][j], stiffnessMatrix[i, j]);

         for (int i = 0; i < 6; i++)
         {
            localVector[i] = GaussTriangle(i, ielem);
         }

         localVector = Math.Abs(DeterminantD()) * localVector;
         AddElementToVector(ielem);

         stiffnessMatrix.Clear();
         massMatrix.Clear();
      }
   }

   private void AddElementToVector(int ielem)
   {
      for (int i = 0; i < 6; i++)
      {
         globalVector[grid.Elements[ielem][i]] += localVector[i];
      }
   }

   private double GaussTriangle(int numberPsi, int ielem)
   {
      
      const double x1a = 0.873821971016996;
      const double x1b = 0.063089014491502;
      const double x2a = 0.501426509658179;
      const double x2b = 0.249286745170910;
      const double x3a = 0.636502499121399;
      const double x3b = 0.310352451033785;
      const double x3c = 0.053145049844816;
      const double w1 = 0.050844906370207;
      const double w2 = 0.116786275726379;
      const double w3 = 0.082851075618374;
      double[] p1 = { x1a, x1b, x1b, x2a, x2b, x2b, x3a, x3b, x3a, x3c, x3b, x3c };
      double[] p2 = { x1b, x1a, x1b, x2b, x2a, x2b, x3b, x3a, x3c, x3a, x3c, x3b };
      double[] w = { w1, w1, w1, w2, w2, w2, w3, w3, w3, w3, w3, w3 };
      double res = 0;

      for (int i = 0; i < w.Length; i++)
      {
         PointRZ point = new();
         point.R = (1 - p1[i] - p2[i]) * vertices[0].R + p1[i] * vertices[1].R + p2[i] * vertices[2].R;
         point.Z = (1 - p1[i] - p2[i]) * vertices[0].Z + p1[i] * vertices[1].Z + p2[i] * vertices[2].Z;

         res += func(point, ielem) * basis(point, numberPsi) * w[i] * 0.5 * point.R;
      }

      return res;
   }

   private double GaussEdge(int numberPsi, Func<PointRZ, int, double> theta, int funcNum, PointRZ fstPoint, PointRZ sndPoint)
   {
      double[] p = { 0.0,
                     1.0 / 3.0 * Math.Sqrt(5 - 2 * Math.Sqrt(10.0 / 7.0)),
                     -1.0 / 3.0 * Math.Sqrt(5 - 2 * Math.Sqrt(10.0 / 7.0)),
                     1.0 / 3.0 * Math.Sqrt(5 + 2 * Math.Sqrt(10.0 / 7.0)),
                     -1.0 / 3.0 * Math.Sqrt(5 + 2 * Math.Sqrt(10.0 / 7.0))};

      double[] w = { 128.0 / 225.0,
                            (322.0 + 13.0 * Math.Sqrt(70.0)) / 900.0,
                            (322.0 + 13.0 * Math.Sqrt(70.0)) / 900.0,
                            (322.0 - 13.0 * Math.Sqrt(70.0)) / 900.0,
                            (322.0 - 13.0 * Math.Sqrt(70.0)) / 900.0 };
      double res = 0;

      double lengthEdge = Math.Sqrt((fstPoint.R - sndPoint.R) * (fstPoint.R - sndPoint.R) +
                            (fstPoint.Z - sndPoint.Z) * (fstPoint.Z - sndPoint.Z));

      for (int i = 0; i < w.Length; i++)
      {
         PointRZ point = new();
         point.R = (sndPoint.R - fstPoint.R) * (1 + p[i]) / 2.0 + fstPoint.R;
         point.Z = (sndPoint.Z - fstPoint.Z) * (1 + p[i]) / 2.0 + fstPoint.Z;                                           

         res += theta(point, funcNum) * basis(point, numberPsi) * w[i] * point.R;
      }

      return lengthEdge * res / 2.0;
   }

   private double DeterminantD()
        => (vertices[1].R - vertices[0].R) * (vertices[2].Z - vertices[0].Z) -
           (vertices[2].R - vertices[0].R) * (vertices[1].Z - vertices[0].Z);

   private void CalcuclateAlphas()
   {
      double dD = DeterminantD();

      alphas[0, 0] = (vertices[1].R * vertices[2].Z - vertices[2].R * vertices[1].Z) / dD;
      alphas[0, 1] = (vertices[1].Z - vertices[2].Z) / dD;
      alphas[0, 2] = (vertices[2].R - vertices[1].R) / dD;

      alphas[1, 0] = (vertices[2].R * vertices[0].Z - vertices[0].R * vertices[2].Z) / dD;
      alphas[1, 1] = (vertices[2].Z - vertices[0].Z) / dD;
      alphas[1, 2] = (vertices[0].R - vertices[2].R) / dD;

      alphas[2, 0] = (vertices[0].R * vertices[1].Z - vertices[1].R * vertices[0].Z) / dD;
      alphas[2, 1] = (vertices[0].Z - vertices[1].Z) / dD;
      alphas[2, 2] = (vertices[1].R - vertices[0].R) / dD;
   }

   private void AssemblyLocalMatrixes()
   {
      double dD = Math.Abs(DeterminantD());
      CalcuclateAlphas();

      stiffnessMatrix[0, 0] = dD *  (alphas[0, 1] * alphas[0, 1] + alphas[0, 2] * alphas[0, 2])
         * (3 * vertices[0].R + vertices[1].R + vertices[2].R) / 10;


      stiffnessMatrix[1, 0] = -dD *  (alphas[0, 1] * alphas[1, 1] + alphas[0, 2] * alphas[1, 2])
         * (2 * vertices[0].R + 2 * vertices[1].R + vertices[2].R) / 30;
      stiffnessMatrix[0, 1] = stiffnessMatrix[1, 0];


      stiffnessMatrix[1, 1] = dD *  (alphas[1, 1] * alphas[1, 1] + alphas[1, 2] * alphas[1, 2])
               * (vertices[0].R + 3 * vertices[1].R + vertices[2].R) / 10;


      stiffnessMatrix[2, 0] = -dD *  (alphas[0, 1] * alphas[2, 1] + alphas[0, 2] * alphas[2, 2])
         * (2 * vertices[0].R + vertices[1].R + 2 * vertices[2].R) / 30;
      stiffnessMatrix[0, 2] = stiffnessMatrix[2, 0];


      stiffnessMatrix[2, 1] = -dD *  (alphas[1, 1] * alphas[2, 1] + alphas[1, 2] * alphas[2, 2])
               * (vertices[0].R + 2 * vertices[1].R + 2 * vertices[2].R) / 30;
      stiffnessMatrix[1, 2] = stiffnessMatrix[2, 1];


      stiffnessMatrix[2, 2] = dD *  (alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2])
         * (vertices[0].R + vertices[1].R + 3 * vertices[2].R) / 10;


      stiffnessMatrix[3, 0] = dD *
         ((3 * alphas[0, 1] * alphas[0, 1] + 14 * alphas[0, 1] * alphas[1, 1] +
         3 * alphas[0, 2] * alphas[0, 2] + 14 * alphas[0, 2] * alphas[1, 2]) * vertices[0].R +
         (-2 * alphas[0, 1] * alphas[0, 1] + 3 * alphas[0, 1] * alphas[1, 1] -
         2 * alphas[0, 2] * alphas[0, 2] + 3 * alphas[0, 2] * alphas[1, 2]) * vertices[1].R +
         (-alphas[0, 1] * alphas[0, 1] + 3 * alphas[0, 1] * alphas[1, 1] -
         alphas[0, 2] * alphas[0, 2] + 3 * alphas[0, 2] * alphas[1, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[0, 3] = stiffnessMatrix[3, 0];


      stiffnessMatrix[3, 1] = dD *
         ((3 * alphas[0, 1] * alphas[1, 1] + 3 * alphas[0, 2] * alphas[1, 2] -
          2 * alphas[1, 1] * alphas[1, 1] - 2 * alphas[1, 2] * alphas[1, 2]) * vertices[0].R +
         (14 * alphas[0, 1] * alphas[1, 1] + 14 * alphas[0, 2] * alphas[1, 2] +
         3 * alphas[1, 1] * alphas[1, 1] + 3 * alphas[1, 2] * alphas[1, 2]) * vertices[1].R +
         (3 * alphas[0, 1] * alphas[1, 1] + 3 * alphas[0, 2] * alphas[1, 2] -
         alphas[1, 1] * alphas[1, 1] - alphas[1, 2] * alphas[1, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[1, 3] = stiffnessMatrix[3, 1];


      stiffnessMatrix[3, 2] = -dD *
         ((alphas[0, 1] * alphas[2, 1] + alphas[0, 2] * alphas[2, 2] +
         2 * alphas[1, 1] * alphas[2, 1] + 2 * alphas[1, 2] * alphas[2, 2]) * vertices[0].R +
         (2 * alphas[0, 1] * alphas[2, 1] + 2 * alphas[0, 2] * alphas[2, 2] +
         alphas[1, 1] * alphas[2, 1] + alphas[1, 2] * alphas[2, 2]) * vertices[1].R +
         (-3 * alphas[0, 1] * alphas[2, 1] - 3 * alphas[0, 2] * alphas[2, 2] -
         3 * alphas[1, 1] * alphas[2, 1] - 3 * alphas[1, 2] * alphas[2, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[2, 3] = stiffnessMatrix[3, 2];


      stiffnessMatrix[3, 3] = dD *  4 *
               ((alphas[0, 1] * alphas[0, 1] + 2 * alphas[0, 1] * alphas[1, 1] +
               alphas[0, 2] * alphas[0, 2] + 2 * alphas[0, 2] * alphas[1, 2] + 3 *
               alphas[1, 1] * alphas[1, 1] + 3 * alphas[1, 2] * alphas[1, 2]) * vertices[0].R +
               (3 * alphas[0, 1] * alphas[0, 1] + 2 * alphas[0, 1] * alphas[1, 1] +
               3 * alphas[0, 2] * alphas[0, 2] + 2 * alphas[0, 2] * alphas[1, 2] +
               alphas[1, 1] * alphas[1, 1] + alphas[1, 2] * alphas[1, 2]) * vertices[1].R +
               (alphas[0, 1] * alphas[0, 1] + alphas[0, 1] * alphas[1, 1] +
               alphas[0, 2] * alphas[0, 2] + alphas[0, 2] * alphas[1, 2] +
               alphas[1, 1] * alphas[1, 1] + alphas[1, 2] * alphas[1, 2]) * vertices[2].R) / 15;


      stiffnessMatrix[4, 0] = dD *
               ((3 * alphas[0, 1] * alphas[1, 1] + 3 * alphas[0, 1] * alphas[2, 1] +
               3 * alphas[0, 2] * alphas[1, 2] + 3 * alphas[0, 2] * alphas[2, 2]) * vertices[0].R +
               (-alphas[0, 1] * alphas[1, 1] - 2 * alphas[0, 1] * alphas[2, 1] -
               alphas[0, 2] * alphas[1, 2] - 2 * alphas[0, 2] * alphas[2, 2]) * vertices[1].R +
               (-2 * alphas[0, 1] * alphas[1, 1] - alphas[0, 1] * alphas[2, 1] -
               2 * alphas[0, 2] * alphas[1, 2] - alphas[0, 2] * alphas[2, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[0, 4] = stiffnessMatrix[4, 0];


      stiffnessMatrix[4, 1] = -dD *
               ((alphas[1, 1] * alphas[1, 1] - 3 * alphas[1, 1] * alphas[2, 1] +
               alphas[1, 2] * alphas[1, 2] - 3 * alphas[1, 2] * alphas[2, 2]) * vertices[0].R +
               (-3 * alphas[1, 1] * alphas[1, 1] - 14 * alphas[1, 1] * alphas[2, 1] -
               3 * alphas[1, 2] * alphas[1, 2] - 14 * alphas[1, 2] * alphas[2, 2]) * vertices[1].R +
               (2 * alphas[1, 1] * alphas[1, 1] - 3 * alphas[1, 1] * alphas[2, 1] +
               2 * alphas[1, 2] * alphas[1, 2] - 3 * alphas[1, 2] * alphas[2, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[1, 4] = stiffnessMatrix[4, 1];


      stiffnessMatrix[4, 2] = dD *
               ((3 * alphas[1, 1] * alphas[2, 1] + 3 * alphas[1, 2] * alphas[2, 2] -
               alphas[2, 1] * alphas[2, 1] - alphas[2, 2] * alphas[2, 2]) * vertices[0].R +
               (3 * alphas[1, 1] * alphas[2, 1] + 3 * alphas[1, 2] * alphas[2, 2] -
               2 * alphas[2, 1] * alphas[2, 1] - 2 * alphas[2, 2] * alphas[2, 2]) * vertices[1].R +
               (14 * alphas[1, 1] * alphas[2, 1] + 14 * alphas[1, 2] * alphas[2, 2] +
               3 * alphas[2, 1] * alphas[2, 1] + 3 * alphas[2, 2] * alphas[2, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[2, 4] = stiffnessMatrix[4, 2];


      stiffnessMatrix[4, 3] = dD *  2 *
               ((alphas[0, 1] * alphas[1, 1] + 2 * alphas[0, 1] * alphas[2, 1] +
               alphas[0, 2] * alphas[1, 2] + 2 * alphas[0, 2] * alphas[2, 2] +
               2 * alphas[1, 1] * alphas[1, 1] + 2 * alphas[1, 1] * alphas[2, 1]
               + 2 * alphas[1, 2] * alphas[1, 2] + 2 * alphas[1, 2] * alphas[2, 2]) * vertices[0].R +
               (2 * alphas[0, 1] * alphas[1, 1] + 6 * alphas[0, 1] * alphas[2, 1] +
               2 * alphas[0, 2] * alphas[1, 2] + 6 * alphas[0, 2] * alphas[2, 2] +
               alphas[1, 1] * alphas[1, 1] + 2 * alphas[1, 1] * alphas[2, 1]
               + alphas[1, 2] * alphas[1, 2] + 2 * alphas[1, 2] * alphas[2, 2]) * vertices[1].R +
               (2 * alphas[0, 1] * alphas[1, 1] + 2 * alphas[0, 1] * alphas[2, 1] +
               2 * alphas[0, 2] * alphas[1, 2] + 2 * alphas[0, 2] * alphas[2, 2] +
               2 * alphas[1, 1] * alphas[1, 1] + alphas[1, 1] * alphas[2, 1] +
               2 * alphas[1, 2] * alphas[1, 2] + alphas[1, 2] * alphas[2, 2]) * vertices[2].R) / 15;
      stiffnessMatrix[3, 4] = stiffnessMatrix[4, 3];


      stiffnessMatrix[4, 4] = dD *  4 *
               ((alphas[1, 1] * alphas[1, 1] + alphas[1, 1] * alphas[2, 1] +
               alphas[1, 2] * alphas[1, 2] + alphas[1, 2] * alphas[2, 2] +
               alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2]) * vertices[0].R +
               (alphas[1, 1] * alphas[1, 1] + 2 * alphas[1, 1] * alphas[2, 1] +
               alphas[1, 2] * alphas[1, 2] + 2 * alphas[1, 2] * alphas[2, 2] +
               3 * alphas[2, 1] * alphas[2, 1] + 3 * alphas[2, 2] * alphas[2, 2]) * vertices[1].R +
               (3 * alphas[1, 1] * alphas[1, 1] + 2 * alphas[1, 1] * alphas[2, 1] +
               3 * alphas[1, 2] * alphas[1, 2] + 2 * alphas[1, 2] * alphas[2, 2] +
               alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2]) * vertices[2].R) / 15;


      stiffnessMatrix[5, 0] = dD *
               ((3 * alphas[0, 1] * alphas[0, 1] + 14 * alphas[0, 1] * alphas[2, 1] +
               3 * alphas[0, 2] * alphas[0, 2] + 14 * alphas[0, 2] * alphas[2, 2]) * vertices[0].R +
               (-alphas[0, 1] * alphas[0, 1] + 3 * alphas[0, 1] * alphas[2, 1] -
               alphas[0, 2] * alphas[0, 2] + 3 * alphas[0, 2] * alphas[2, 2]) * vertices[1].R +
               (-2 * alphas[0, 1] * alphas[0, 1] + 3 * alphas[0, 1] * alphas[2, 1] -
               2 * alphas[0, 2] * alphas[0, 2] + 3 * alphas[0, 2] * alphas[2, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[0, 5] = stiffnessMatrix[5, 0];


      stiffnessMatrix[5, 1] = -dD *
               ((alphas[0, 1] * alphas[1, 1] + alphas[0, 2] * alphas[1, 2] +
               2 * alphas[1, 1] * alphas[2, 1] + 2 * alphas[1, 2] * alphas[2, 2]) * vertices[0].R +
               (-3 * alphas[0, 1] * alphas[1, 1] - 3 * alphas[0, 2] * alphas[1, 2] -
               3 * alphas[1, 1] * alphas[2, 1] - 3 * alphas[1, 2] * alphas[2, 2]) * vertices[1].R +
               (2 * alphas[0, 1] * alphas[1, 1] + 2 * alphas[0, 2] * alphas[1, 2] +
               alphas[1, 1] * alphas[2, 1] + alphas[1, 2] * alphas[2, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[1, 5] = stiffnessMatrix[5, 1];


      stiffnessMatrix[5, 2] = dD *
         ((3 * alphas[0, 1] * alphas[2, 1] + 3 * alphas[0, 2] * alphas[2, 2] -
         2 * alphas[2, 1] * alphas[2, 1] - 2 * alphas[2, 2] * alphas[2, 2]) * vertices[0].R +
         (3 * alphas[0, 1] * alphas[2, 1] + 3 * alphas[0, 2] * alphas[2, 2] -
         alphas[2, 1] * alphas[2, 1] - alphas[2, 2] * alphas[2, 2]) * vertices[1].R +
         (14 * alphas[0, 1] * alphas[2, 1] + 14 * alphas[0, 2] * alphas[2, 2] +
         3 * alphas[2, 1] * alphas[2, 1] + 3 * alphas[2, 2] * alphas[2, 2]) * vertices[2].R) / 30;
      stiffnessMatrix[2, 5] = stiffnessMatrix[5, 2];


      stiffnessMatrix[5, 3] = dD *  2 *
               ((alphas[0, 1] * alphas[0, 1] + 2 * alphas[0, 1] * alphas[1, 1] +
               2 * alphas[0, 1] * alphas[2, 1] + alphas[0, 2] * alphas[0, 2] +
               2 * alphas[0, 2] * alphas[1, 2] + 2 * alphas[0, 2] * alphas[2, 2] +
               6 * alphas[1, 1] * alphas[2, 1] + 6 * alphas[1, 2] * alphas[2, 2]) * vertices[0].R +
               (2 * alphas[0, 1] * alphas[0, 1] + alphas[0, 1] * alphas[1, 1] +
               2 * alphas[0, 1] * alphas[2, 1] + 2 * alphas[0, 2] * alphas[0, 2] +
               alphas[0, 2] * alphas[1, 2] + 2 * alphas[0, 2] * alphas[2, 2] +
               2 * alphas[1, 1] * alphas[2, 1] + 2 * alphas[1, 2] * alphas[2, 2]) * vertices[1].R +
               (2 * alphas[0, 1] * alphas[0, 1] + 2 * alphas[0, 1] * alphas[1, 1] +
               alphas[0, 1] * alphas[2, 1] + 2 * alphas[0, 2] * alphas[0, 2] +
               2 * alphas[0, 2] * alphas[1, 2] + alphas[0, 2] * alphas[2, 2] +
               2 * alphas[1, 1] * alphas[2, 1] + 2 * alphas[1, 2] * alphas[2, 2]) * vertices[2].R) / 15;
      stiffnessMatrix[3, 5] = stiffnessMatrix[5, 3];


      stiffnessMatrix[5, 4] = dD *  2 *
               ((2 * alphas[0, 1] * alphas[1, 1] + alphas[0, 1] * alphas[2, 1] +
               2 * alphas[0, 2] * alphas[1, 2] + alphas[0, 2] * alphas[2, 2] +
               2 * alphas[1, 1] * alphas[2, 1] + 2 * alphas[1, 2] * alphas[2, 2] +
               2 * alphas[2, 1] * alphas[2, 1] + 2 * alphas[2, 2] * alphas[2, 2]) * vertices[0].R +
               (2 * alphas[0, 1] * alphas[1, 1] + 2 * alphas[0, 1] * alphas[2, 1] +
               2 * alphas[0, 2] * alphas[1, 2] + 2 * alphas[0, 2] * alphas[2, 2] +
               alphas[1, 1] * alphas[2, 1] + alphas[1, 2] * alphas[2, 2] +
               2 * alphas[2, 1] * alphas[2, 1] + 2 * alphas[2, 2] * alphas[2, 2]) * vertices[1].R +
               (6 * alphas[0, 1] * alphas[1, 1] + 2 * alphas[0, 1] * alphas[2, 1] +
               6 * alphas[0, 2] * alphas[1, 2] + 2 * alphas[0, 2] * alphas[2, 2] +
               2 * alphas[1, 1] * alphas[2, 1] + 2 * alphas[1, 2] * alphas[2, 2] +
               alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2]) * vertices[2].R) / 15;
      stiffnessMatrix[4, 5] = stiffnessMatrix[5, 4];

      stiffnessMatrix[5, 5] = dD *  4 *
               ((alphas[0, 1] * alphas[0, 1] + 2 * alphas[0, 1] * alphas[2, 1] +
               alphas[0, 2] * alphas[0, 2] + 2 * alphas[0, 2] * alphas[2, 2] +
               3 * alphas[2, 1] * alphas[2, 1] + 3 * alphas[2, 2] * alphas[2, 2]) * vertices[0].R +
               (alphas[0, 1] * alphas[0, 1] + alphas[0, 1] * alphas[2, 1] +
               alphas[0, 2] * alphas[0, 2] + alphas[0, 2] * alphas[2, 2] +
               alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2]) * vertices[1].R +
               (3 * alphas[0, 1] * alphas[0, 1] + 2 * alphas[0, 1] * alphas[2, 1] +
               3 * alphas[0, 2] * alphas[0, 2] + 2 * alphas[0, 2] * alphas[2, 2] +
               alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2]) * vertices[2].R) / 15;


      massMatrix[0, 0] = dD * (5 * vertices[0].R + vertices[1].R + vertices[2].R) / 420;


      massMatrix[1, 0] = -dD * (4 * vertices[0].R + 4 * vertices[1].R - vertices[2].R) / 2520;
      massMatrix[0, 1] = massMatrix[1, 0];


      massMatrix[1, 1] = dD * (vertices[0].R + 5 * vertices[1].R + vertices[2].R) / 420;


      massMatrix[2, 0] = -dD * (4 * vertices[0].R - vertices[1].R + 4 * vertices[2].R) / 2520;
      massMatrix[0, 2] = massMatrix[2, 0];


      massMatrix[2, 1] = dD * (vertices[0].R - 4 * vertices[1].R - 4 * vertices[2].R) / 2520;
      massMatrix[1, 2] = massMatrix[2, 1];

      massMatrix[2, 2] = dD * (vertices[0].R + vertices[1].R + 5 * vertices[2].R) / 420;


      massMatrix[3, 0] = dD * (3 * vertices[0].R - 2 * vertices[1].R - vertices[2].R) / 630;
      massMatrix[0, 3] = massMatrix[3, 0];


      massMatrix[3, 1] = -dD * (2 * vertices[0].R - 3 * vertices[1].R + vertices[2].R) / 630;
      massMatrix[1, 3] = massMatrix[3, 1];


      massMatrix[3, 2] = -dD * (3 * vertices[0].R + 3 * vertices[1].R + vertices[2].R) / 630;
      massMatrix[2, 3] = massMatrix[3, 2];


      massMatrix[3, 3] = dD * 4 * (3 * vertices[0].R + 3 * vertices[1].R + vertices[2].R) / 315;


      massMatrix[4, 0] = -dD * (vertices[0].R + 3 * vertices[1].R + 3 * vertices[2].R) / 630;
      massMatrix[0, 4] = massMatrix[4, 0];


      massMatrix[4, 1] = -dD * (vertices[0].R - 3 * vertices[1].R + 2 * vertices[2].R) / 630;
      massMatrix[1, 4] = massMatrix[4, 1];


      massMatrix[4, 2] = -dD * (vertices[0].R + 2 * vertices[1].R - 3 * vertices[2].R) / 630;
      massMatrix[2, 4] = massMatrix[4, 2];


      massMatrix[4, 3] = dD * 2 * (2 * vertices[0].R + 3 * vertices[1].R + 2 * vertices[2].R) / 315;
      massMatrix[3, 4] = massMatrix[4, 3];


      massMatrix[4, 4] = dD * 4 * (vertices[0].R + 3 * vertices[1].R + 3 * vertices[2].R) / 315;


      massMatrix[5, 0] = dD * (3 * vertices[0].R - vertices[1].R - 2 * vertices[2].R) / 630;
      massMatrix[0, 5] = massMatrix[5, 0];


      massMatrix[5, 1] = -dD * (3 * vertices[0].R + vertices[1].R + 3 * vertices[2].R) / 630;
      massMatrix[1, 5] = massMatrix[5, 1];


      massMatrix[5, 2] = -dD * (2 * vertices[0].R + vertices[1].R - 3 * vertices[2].R) / 630;
      massMatrix[2, 5] = massMatrix[5, 2];


      massMatrix[5, 3] = dD * 2 * (3 * vertices[0].R + 2 * vertices[1].R + 2 * vertices[2].R) / 315;
      massMatrix[3, 5] = massMatrix[5, 3];


      massMatrix[5, 4] = dD * 2 * (2 * vertices[0].R + 2 * vertices[1].R + 3 * vertices[2].R) / 315;
      massMatrix[4, 5] = massMatrix[5, 4];


      massMatrix[5, 5] = dD * 4 * (3 * vertices[0].R + vertices[1].R + 3 * vertices[2].R) / 315;
   }

   private double basis(PointRZ point, int numPsi)
   {
      double l1 = alphas[0, 0] + alphas[0, 1] * point.R + alphas[0, 2] * point.Z;
      double l2 = alphas[1, 0] + alphas[1, 1] * point.R + alphas[1, 2] * point.Z;
      double l3 = alphas[2, 0] + alphas[2, 1] * point.R + alphas[2, 2] * point.Z;
      
      switch (numPsi)
      {
         case 0:
            return l1 * (2 * l1 - 1);
         case 1:
            return l2 * (2 * l2 - 1);
         case 2:
            return l3 * (2 * l3 - 1);
         case 3:
            return 4 * l1 * l2;
         case 4:
            return 4 * l2 * l3;
         case 5:
            return 4 * l3 * l1;
         default:
            return 0;
      }
   }

   private int FindElement(PointRZ point)
   {
      for (int ielem = 0; ielem < grid.Elements.Length; ielem++)
      {
         vertices[0] = grid.Node[grid.Elements[ielem][0]];
         vertices[1] = grid.Node[grid.Elements[ielem][1]];
         vertices[2] = grid.Node[grid.Elements[ielem][2]];

         double s01 = Math.Abs((vertices[1].R - vertices[0].R) * (point.Z - vertices[0].Z) -
                  (point.R - vertices[0].R) * (vertices[1].Z - vertices[0].Z));

         double s12 = Math.Abs((vertices[2].R - vertices[1].R) * (point.Z - vertices[1].Z) -
                  (point.R - vertices[1].R) * (vertices[2].Z - vertices[1].Z));

         double s20 = Math.Abs((vertices[0].R - vertices[2].R) * (point.Z - vertices[2].Z) -
                  (point.R - vertices[2].R) * (vertices[0].Z - vertices[2].Z));

         double dD = Math.Abs(DeterminantD());

         if (dD == (s01 + s12 + s20))
            return ielem;
      }
      return -1;
   }
   public double ValueAtPoint(PointRZ point)
   {
      double res = 0;

      int ielem = FindElement(point);
      CalcuclateAlphas();
      if (ielem != -1)
      {
         for (int i = 0; i < 6; i++)
         {
            res += slae.solution[grid.Elements[ielem][i]] * basis(point, i);
         }
      }
      return res;
   }
}
