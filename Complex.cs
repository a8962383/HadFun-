using System;

public struct Complex
{
    private float _real;
    private float _imaginary;

    public Complex(float real, float imaginary)
    {
        _real = real;
        _imaginary = imaginary;
    }

    public float Real
    {
        get => _real;
        set => _real = value;
    }

    public float Imaginary
    {
        get => _imaginary;
        set => _imaginary = value;
    }

    public override string ToString()
    {
        return string.Format("({0}, {1}i)", _real, _imaginary);
    }

    public static bool operator ==(Complex c1, Complex c2)
    {
        return c1._real == c2._real && c1._imaginary == c2._imaginary;
    }

    public static bool operator !=(Complex c1, Complex c2)
    {
        return !(c1 == c2);
    }

    public override bool Equals(object o2)
    {
        if (!(o2 is Complex c2))
        {
            return false;
        }

        return this == c2;
    }

    public override int GetHashCode()
    {
        return _real.GetHashCode() ^ _imaginary.GetHashCode();
    }

    public static Complex operator +(Complex c1, Complex c2)
    {
        return new Complex(c1._real + c2._real, c1._imaginary + c2._imaginary);
    }

    public static Complex operator -(Complex c1, Complex c2)
    {
        return new Complex(c1._real - c2._real, c1._imaginary - c2._imaginary);
    }

    /// <summary>
    /// Product of two complex numbers.
    /// </summary>
    public static Complex operator *(Complex c1, Complex c2)
    {
        return new Complex(
            c1._real * c2._real - c1._imaginary * c2._imaginary,
            c1._real * c2._imaginary + c2._real * c1._imaginary);
    }

    /// <summary>
    /// Quotient of two complex numbers.
    /// </summary>
    public static Complex operator /(Complex c1, Complex c2)
    {
        if (c2._real == 0.0f && c2._imaginary == 0.0f)
        {
            throw new DivideByZeroException("Can't divide by zero Complex number");
        }

        float denominator = c2._real * c2._real + c2._imaginary * c2._imaginary;
        float newReal = (c1._real * c2._real + c1._imaginary * c2._imaginary) / denominator;
        float newImaginary = (c2._real * c1._imaginary - c1._real * c2._imaginary) / denominator;

        return new Complex(newReal, newImaginary);
    }

    /// <summary>
    /// Non-operator version of addition.
    /// </summary>
    public static Complex Add(Complex c1, Complex c2)
    {
        return c1 + c2;
    }

    /// <summary>
    /// Non-operator version of subtraction.
    /// </summary>
    public static Complex Subtract(Complex c1, Complex c2)
    {
        return c1 - c2;
    }

    /// <summary>
    /// Non-operator version of multiplication.
    /// </summary>
    public static Complex Multiply(Complex c1, Complex c2)
    {
        return c1 * c2;
    }

    /// <summary>
    /// Non-operator version of division.
    /// </summary>
    public static Complex Divide(Complex c1, Complex c2)
    {
        return c1 / c2;
    }

    /// <summary>
    /// Calculates the magnitude of a complex number.
    /// </summary>
    public static float Magnitude(Complex c)
    {
        return (float)Math.Sqrt(c._real * c._real + c._imaginary * c._imaginary);
    }
}