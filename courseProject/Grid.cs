using System.Reflection.Emit;

namespace courseProject;

public class Grid
{
   private double rStart;
   private double rEnd;
   private double rStep;
   private double zStart;
   private double zEnd;
   private double zStep;
   private int rSteps;
   private int zSteps;
   private int nodesInRow;
   private int nodesInCol;
   public PointRZ[] Node { get; set; }
   public int[][] Elements { get; set; }
   public double Gamma { get; init; }
   public double[] Lambda { get; init; }
   private double[] materials = { 1, 10, 20, 5 };

   public Grid(string path)
   {
      using (StreamReader sr = new (path))
      {
         string[] data;
         data = sr.ReadLine()!.Split(" ").ToArray();
         rStart = Convert.ToDouble(data[0]);
         rEnd = Convert.ToDouble(data[1]);
         rSteps = Convert.ToInt32(data[2]);
         data = sr.ReadLine()!.Split(" ").ToArray();
         zStart = Convert.ToDouble(data[0]);
         zEnd = Convert.ToDouble(data[1]);
         zSteps = Convert.ToInt32(data[2]);
         data = sr.ReadLine()!.Split(" ").ToArray();
         Lambda = new double[2 * rSteps * zSteps];
         if (data.Length < Lambda.Length)
         {
            for (int i = 0; i < Lambda.Length; i++)
               Lambda[i] = Convert.ToDouble(data[0]);
         }
         else
         {
            for (int i = 0; i < Lambda.Length; i++)
               Lambda[i] = Convert.ToDouble(data[i]);
         }
         Gamma = Convert.ToDouble(sr.ReadLine());
      }

      rStep = (rEnd - rStart) / rSteps;
      zStep = (zEnd - zStart) / zSteps;

      nodesInRow = 2 * rSteps + 1;
      nodesInCol = 2 * zSteps + 1;

      Node = new PointRZ[nodesInRow * nodesInCol];
      int index = 0;
      for (double z = zStart; z <= zEnd; z+= zStep/2)
      {
         for (double r = rStart; r <= rEnd; r += rStep/2)
         {
            Node[index] = new(r, z);
            index++;
         }
      }
      
      index = 0;
      Elements = new int[2*rSteps*zSteps].Select(_ => new int[6]).ToArray();
      for (int j = 0; j < zSteps; j++)
      {
         for (int i = 0; i < rSteps; i++)
         {
            Elements[index][0] = 2 * i + 2 * nodesInRow * j;
            Elements[index][1] = 2 * i + 2 * nodesInRow * j + 2;
            Elements[index][2] = 2 * i + 2 * nodesInRow * j + 2 + 2 * nodesInRow;
            Elements[index][3] = 2 * i + 2 * nodesInRow * j + 1;
            Elements[index][4] = 2 * i + 2 * nodesInRow * j + 2 + nodesInRow;
            Elements[index++][5] = 2 * i + 2 * nodesInRow * j + 1 + nodesInRow;


            Elements[index][0] = 2 * i + 2 * nodesInRow * j;
            Elements[index][1] = 2 * i + 2 * nodesInRow * j + 2 * nodesInRow;
            Elements[index][2] = 2 * i + 2 * nodesInRow * j + 2 + 2 * nodesInRow;
            Elements[index][3] = 2 * i + 2 * nodesInRow * j + nodesInRow;
            Elements[index][4] = 2 * i + 2 * nodesInRow * j + 1 + 2 * nodesInRow;
            Elements[index++][5] = 2 * i + 2 * nodesInRow * j + 1 + nodesInRow;
         }
      }
   }
}
