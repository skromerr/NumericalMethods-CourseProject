namespace courseProject;

public class SLAE
{
   private SparseMatrix matrix;
   private Vector vector;
   public Vector solution = default!;
   private double eps;
   private int maxIter;

   public SLAE(int maxIter, double eps, Vector vector, SparseMatrix matrix)
   {
      this.eps = eps;
      this.maxIter = maxIter;
      this.vector = vector;
      this.matrix = matrix;
   }

   public void CGM()
   {
      double vectorNorm = vector.Norm();

      solution = new(vector.Length);

      Vector z = new(vector.Length);

      Vector r = vector - matrix * solution;
      Vector.Copy(r, z);

      int iter;

      for (iter = 0; iter < maxIter && r.Norm() / vectorNorm >= eps; iter++)
      {
         var tmp = matrix * z;
         var alpha = r * r / (tmp * z);
         solution += alpha * z;
         var squareNorm = r * r;
         r -= alpha * tmp;
         var beta = r * r / squareNorm;
         z = r + beta * z;
      }

      //Console.WriteLine($"Last iteration - {iter}\n" +
      //   $"Residual norm - {r.Norm() / vectorNorm}\n {eps}");
   }

   public void CGMWithMax()
   {
      double vectorNorm = vector.Norm();

      solution = new(vector.Length);

      Vector z = new(vector.Length);

      Vector r = vector - matrix * solution;
      Vector.Copy(r, z);

      for (int iter = 0; iter < maxIter && r.Norm() / vectorNorm >= eps; iter++)
      {
         var tmp = matrix * z;
         var alpha = r * r / (tmp * z);
         for (int i = 0; i < vector.Length; i++)
         {
            if (matrix.Di[i] == Const.max)
            {
               solution[i] = vector[i] / Const.max;
            }
            else
            {
               solution[i] += alpha * z[i];
            }
         }
         var squareNorm = r * r;
         for (int i = 0; i < vector.Length; i++)
         {
            if (matrix.Di[i] == Const.max)
            {
               r[i] = 0;
            }
            else
            {
               r[i] -= alpha * tmp[i];
            }
         }
         var beta = r * r / squareNorm;
         z = r + beta * z;
      }
   }

   public void PrintSolution()
   {
      for(int i = 0; i < solution.Length; i++)
      {
         Console.WriteLine(solution[i]);
      }
   }
}
