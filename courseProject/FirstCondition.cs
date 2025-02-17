using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace courseProject;

public class FirstCondition
{
   public int FuncNumber { get; set; }
   public PointRZ point { get; set; }
   public int NodeNumber { get; set; }

   public FirstCondition(PointRZ node, int nodeNumber, int funcNumber)
   {
      point = node;
      NodeNumber = nodeNumber;
      FuncNumber = funcNumber;
   }

   public double Ug()
   {
      switch(FuncNumber)
      {
         case 0:
            return 0;
         case 1:
            return point.R;
         case 2:
            return point.R;
         default:
            return point.Z;
      }
   }
}
