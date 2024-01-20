namespace Wacton.Unicolour
{

    internal static class Adaptation
    {
        private static readonly Matrix Bradford = new(new[,]
        {
            { +0.8951, +0.2664, -0.1614 },
            { -0.7502, +1.7135, +0.0367 },
            { +0.0389, -0.0685, +1.0296 }
        });

        internal static Matrix WhitePoint(Matrix matrix, WhitePoint sourceWhitePoint, WhitePoint destinationWhitePoint)
        {
            if (sourceWhitePoint == destinationWhitePoint)
            {
                return matrix;
            }

            var adaptedBradford = AdaptedBradfordMatrix(sourceWhitePoint, destinationWhitePoint);
            return adaptedBradford.Multiply(matrix);
        }

        // http://www.brucelindbloom.com/index.html?Eqn_ChromAdapt.html
        private static Matrix AdaptedBradfordMatrix(WhitePoint sourceWhitePoint, WhitePoint destinationWhitePoint)
        {
            var sourceWhite = sourceWhitePoint.AsXyzMatrix();
            var destinationWhite = destinationWhitePoint.AsXyzMatrix();

            var sourceLms = Bradford.Multiply(sourceWhite).ToTriplet();
            var destinationLms = Bradford.Multiply(destinationWhite).ToTriplet();

            var lmsRatios = new Matrix(new[,]
            {
                { destinationLms.First / sourceLms.First, 0, 0 },
                { 0, destinationLms.Second / sourceLms.Second, 0 },
                { 0, 0, destinationLms.Third / sourceLms.Third }
            });

            var inverseBradford = Bradford.Inverse();
            var adaptedBradfordMatrix = inverseBradford.Multiply(lmsRatios).Multiply(Bradford);
            return adaptedBradfordMatrix;
        }
    }
}