namespace courseProject;

public class PointRZ
{
   public double R { get; set; }
   public double Z { get; set; }

   public PointRZ()
   {
      R = 0;
      Z = 0;
   }
   public PointRZ(double r, double z)
   {
      R = r;
      Z = z;
   }
}
