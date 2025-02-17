using System.Xml.Linq;

namespace courseProject;

public class SecondCondition
{
   public int ElemNumber { get; set; }
   public int EdgeType { get; set; }         // 0 - horizontal, 1 - vertical
   public int FuncNumber { get; set; }

   public SecondCondition(int elemNumber, int edgeType, int funcNumber)
   {
      ElemNumber = elemNumber;
      EdgeType = edgeType;
      FuncNumber = funcNumber;
   }

   public double Theta(PointRZ point, int funcNum)
   {
      switch (funcNum)  // 0 - bottom, 1 - right, 2 - top, 3 - left
      {
         case 0:
            return 0;
         case 1:
            return 1;
         case 2:
            return 0;
         case 3:
            return -1;
         default:
            return 0;
      }
   }
}
