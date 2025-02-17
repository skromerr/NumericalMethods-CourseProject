namespace courseProject.Trash;



//private void AssemblyLocalMatrixesNikita()
//{
//   double dD = Math.Abs(DeterminantD());
//   CalcuclateAlphas();

//   //rs=[dl1^2 dl1dl2 dl1dl3 dl2^2 dl2dl3 dl3dl3]
//   //каждая из 6 пар дифференциалов состоит из 3 составляющих (т.к. вектор r=r1L1+r2L2+r3L3)
//   for (int i = 0; i < 6; i++)
//   {
//      for (int j = 0; j < 6; j++)
//      {
//         stiffnessMatrix[i, j] += matrixG[i][j][0][0] * vertices[0].R * grid.Lambda *
//                                   (alphas[0, 1] * alphas[0, 1] + alphas[0, 2] * alphas[0, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][0][1] * vertices[1].R * grid.Lambda *
//                                            (alphas[0, 1] * alphas[0, 1] + alphas[0, 2] * alphas[0, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][0][2] * vertices[2].R * grid.Lambda *
//                                            (alphas[0, 1] * alphas[0, 1] + alphas[0, 2] * alphas[0, 2]) * dD;

//         stiffnessMatrix[i, j] += matrixG[i][j][1][0] * vertices[0].R * grid.Lambda *
//                                            (alphas[0, 1] * alphas[1, 1] + alphas[0, 2] * alphas[1, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][1][1] * vertices[1].R * grid.Lambda *
//                                            (alphas[0, 1] * alphas[1, 1] + alphas[0, 2] * alphas[1, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][1][2] * vertices[2].R * grid.Lambda *
//                                            (alphas[0, 1] * alphas[1, 1] + alphas[0, 2] * alphas[1, 2]) * dD;

//         stiffnessMatrix[i, j] += matrixG[i][j][2][0] * vertices[0].R * grid.Lambda *
//                                            (alphas[0, 1] * alphas[2, 1] + alphas[0, 2] * alphas[2, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][2][1] * vertices[1].R * grid.Lambda *
//                                            (alphas[0, 1] * alphas[2, 1] + alphas[0, 2] * alphas[2, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][2][2] * vertices[2].R * grid.Lambda *
//                                            (alphas[0, 1] * alphas[2, 1] + alphas[0, 2] * alphas[2, 2]) * dD;

//         stiffnessMatrix[i, j] += matrixG[i][j][3][0] * vertices[0].R * grid.Lambda *
//                                            (alphas[1, 1] * alphas[1, 1] + alphas[1, 2] * alphas[1, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][3][1] * vertices[1].R * grid.Lambda *
//                                            (alphas[1, 1] * alphas[1, 1] + alphas[1, 2] * alphas[1, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][3][2] * vertices[2].R * grid.Lambda *
//                                            (alphas[1, 1] * alphas[1, 1] + alphas[1, 2] * alphas[1, 2]) * dD;

//         stiffnessMatrix[i, j] += matrixG[i][j][4][0] * vertices[0].R * grid.Lambda *
//                                            (alphas[1, 1] * alphas[2, 1] + alphas[1, 2] * alphas[2, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][4][1] * vertices[1].R * grid.Lambda *
//                                            (alphas[1, 1] * alphas[2, 1] + alphas[1, 2] * alphas[2, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][4][2] * vertices[2].R * grid.Lambda *
//                                            (alphas[1, 1] * alphas[2, 1] + alphas[1, 2] * alphas[2, 2]) * dD;

//         stiffnessMatrix[i, j] += matrixG[i][j][5][0] * vertices[0].R * grid.Lambda *
//                                            (alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][5][1] * vertices[1].R * grid.Lambda *
//                                            (alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2]) * dD;
//         stiffnessMatrix[i, j] += matrixG[i][j][5][2] * vertices[2].R * grid.Lambda *
//                                            (alphas[2, 1] * alphas[2, 1] + alphas[2, 2] * alphas[2, 2]) * dD;
//      }
//   }

//   for (int i = 0; i < 6; i++)
//   {
//      for (int j = 0; j < 6; j++)
//      {
//         for (int k = 0; k < 3; k++)
//         {
//            massMatrix[i, j] += matrixM[i][j][k] * vertices[k].R * dD;
//         }
//      }
//   }
//}
//--------------------------------------------------------------
//private double[][][][] matrixG = default!;
//private double[][][] matrixM = default!;
//---------------------------------------------------------------
//matrixM = new double[6].Select(_ => new double[6].ToArray().Select(_ => new double[3]).ToArray()).ToArray();
//matrixG = new double[6].Select(_ => new double[6].ToArray().Select(_ => new double[6].ToArray()
//      .Select(_ => new double[3]).ToArray()).ToArray()).ToArray();

//using StreamReader sr1 = new("C:/Users/Skromer/source/repos/courseProject/courseProject/Grz.txt"),
//   sr2 = new("C:/Users/Skromer/source/repos/courseProject/courseProject/Mrz.txt");
//string[] vars;

//for (int i = 0; i < 216; i++)
//{
//   vars = sr1.ReadLine()!.Split(" ").ToArray();

//   matrixG[int.Parse(vars[0])][int.Parse(vars[1])][int.Parse(vars[2])][0] = double.Parse(vars[3]);
//   matrixG[int.Parse(vars[0])][int.Parse(vars[1])][int.Parse(vars[2])][1] = double.Parse(vars[4]);
//   matrixG[int.Parse(vars[0])][int.Parse(vars[1])][int.Parse(vars[2])][2] = double.Parse(vars[5]);
//}

//for (int i = 0; i < 36; i++)
//{
//   vars = sr2.ReadLine()!.Split(" ").ToArray();

//   matrixM[int.Parse(vars[0])][int.Parse(vars[1])][0] = double.Parse(vars[2]);
//   matrixM[int.Parse(vars[0])][int.Parse(vars[1])][1] = double.Parse(vars[3]);
//   matrixM[int.Parse(vars[0])][int.Parse(vars[1])][2] = double.Parse(vars[4]);
//}

