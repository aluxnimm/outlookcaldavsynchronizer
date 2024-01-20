using System;

namespace Wacton.Unicolour
{

    internal class Matrix
    {
        internal double[,] Data { get; }
        internal double this[int row, int col] => Data[row, col];
        private int Rows => Data.GetLength(0);
        private int Cols => Data.GetLength(1);

        internal Matrix(double[,] data)
        {
            Data = data;
        }

        internal Matrix Multiply(Matrix other)
        {
            if (other.Rows != Cols)
            {
                throw new ArgumentException(
                    $"Cannot multiply {this} matrix by {other} matrix, incompatible dimensions");
            }

            var dimension = Cols;
            var rows = Rows;
            var cols = other.Cols;

            var result = new double[rows, cols];
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    result[row, col] = 0;
                    for (var i = 0; i < dimension; i++)
                    {
                        result[row, col] += this[row, i] * other[i, col];
                    }
                }
            }

            return new Matrix(result);
        }

        internal Matrix Inverse()
        {
            if (Rows != 3 || Cols != 3)
            {
                throw new InvalidOperationException("Only inverse of 3x3 matrix is supported");
            }

            var a = this[0, 0];
            var b = this[0, 1];
            var c = this[0, 2];
            var d = this[1, 0];
            var e = this[1, 1];
            var f = this[1, 2];
            var g = this[2, 0];
            var h = this[2, 1];
            var i = this[2, 2];

            var determinant = a * e * i + b * f * g + c * d * h - c * e * g - a * f * h - b * d * i;
            var adjugate = new[,]
            {
                { e * i - f * h, h * c - i * b, b * f - c * e },
                { g * f - d * i, a * i - g * c, d * c - a * f },
                { d * h - g * e, g * b - a * h, a * e - d * b }
            };

            var inverse = new double[3, 3];
            for (var row = 0; row < 3; row++)
            {
                for (var col = 0; col < 3; col++)
                {
                    inverse[row, col] = determinant == 0
                        ? double.NaN
                        : adjugate[row, col] / determinant;
                }
            }

            return new Matrix(inverse);
        }

        internal Matrix Scale(double scalar) => Select(x => x * scalar);

        internal Matrix Select(Func<double, double> operation)
        {
            var result = new double[Rows, Cols];
            for (var row = 0; row < Rows; row++)
            {
                for (var col = 0; col < Cols; col++)
                {
                    result[row, col] = operation(this[row, col]);
                }
            }

            return new Matrix(result);
        }

        internal ColourTriplet ToTriplet()
        {
            if (Rows != 3 || Cols != 1)
            {
                throw new InvalidOperationException("Can only create triplet from 3x1 matrix");
            }

            return new ColourTriplet(Data[0, 0], Data[1, 0], Data[2, 0]);
        }

        internal static Matrix FromTriplet(ColourTriplet triplet) =>
            FromTriplet(triplet.First, triplet.Second, triplet.Third);

        internal static Matrix FromTriplet(double first, double second, double third)
        {
            return new Matrix(new[,]
            {
                { first },
                { second },
                { third }
            });
        }

        public override string ToString() => $"{Rows}x{Cols}";
    }
}