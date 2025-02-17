using courseProject;

Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
Grid grid = new("C:/Users/Skromer/source/repos/courseProject/courseProject/grid.txt");
FEM fem = new(grid);
fem.ComputeWithMax();
fem.PrintSolution();
Console.WriteLine();
PointRZ points = new PointRZ(1.3, 2.3);
double res = fem.ValueAtPoint(points);

Console.WriteLine($"Значение функции в точке ({points.R};{points.Z}) равно {res}.");
res = fem.U(points);
Console.WriteLine($"Точное значение функции в точке ({points.R};{points.Z}) равно {res}.");