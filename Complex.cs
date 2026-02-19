using System;

/// <summary>
/// Represents a complex number with single-precision floating-point
/// real and imaginary components.
/// </summary>
/// <remarks>
/// <para>
/// Provides arithmetic operators (<c>+</c>, <c>-</c>, <c>*</c>, <c>/</c>),
/// equality operators (<c>==</c>, <c>!=</c>), and static helper methods
/// (<see cref="Add"/>, <see cref="Subtract"/>, <see cref="Multiply"/>,
/// <see cref="Divide"/>, <see cref="Magnitude"/>).
/// </para>
/// <para>
/// <b>Note:</b> This is a learning implementation. For production use,
/// prefer <see cref="System.Numerics.Complex"/> which uses <c>double</c>
/// precision and offers a richer API.
/// </para>
/// </remarks>
public struct Complex
{
    private float _real;
    private float _imaginary;

    /// <summary>
    /// Initializes a new <see cref="Complex"/> number.
    /// </summary>
    /// <param name="real">The real component.</param>
    /// <param name="imaginary">The imaginary component.</param>
    public Complex(float real, float imaginary)
    {
        _real = real;
        _imaginary = imaginary;
    }

    /// <summary>Gets or sets the real component of the complex number.</summary>
    public float Real
    {
        get => _real;
        set => _real = value;
    }

    /// <summary>Gets or sets the imaginary component of the complex number.</summary>
    public float Imaginary
    {
        get => _imaginary;
        set => _imaginary = value;
    }

    /// <summary>
    /// Returns a string representation in the form <c>(real, imaginaryi)</c>.
    /// </summary>
    /// <returns>A formatted string such as <c>(3, 4i)</c>.</returns>
    public override string ToString()
    {
        return string.Format("({0}, {1}i)", _real, _imaginary);
    }

    /// <summary>
    /// Determines whether two <see cref="Complex"/> numbers are equal by
    /// comparing their real and imaginary components.
    /// </summary>
    /// <remarks>
    /// Uses exact <c>float</c> equality. For approximate comparisons,
    /// consider an epsilon-based check.
    /// </remarks>
    public static bool operator ==(Complex c1, Complex c2)
    {
        return c1._real == c2._real && c1._imaginary == c2._imaginary;
    }

    /// <summary>
    /// Determines whether two <see cref="Complex"/> numbers are not equal.
    /// </summary>
    public static bool operator !=(Complex c1, Complex c2)
    {
        return !(c1 == c2);
    }

    /// <summary>
    /// Determines whether this instance is equal to another object.
    /// </summary>
    /// <param name="o2">The object to compare with.</param>
    /// <returns><c>true</c> if <paramref name="o2"/> is a <see cref="Complex"/> with equal components.</returns>
    public override bool Equals(object o2)
    {
        if (!(o2 is Complex c2))
        {
            return false;
        }

        return this == c2;
    }

    /// <summary>
    /// Returns a hash code for this complex number.
    /// </summary>
    /// <remarks>
    /// Currently uses XOR of the component hash codes, which produces
    /// identical hashes for <c>(a, b)</c> and <c>(b, a)</c>. Consider using
    /// <see cref="HashCode.Combine{T1, T2}(T1, T2)"/> for better distribution.
    /// </remarks>
    public override int GetHashCode()
    {
        return _real.GetHashCode() ^ _imaginary.GetHashCode();
    }

    /// <summary>
    /// Adds two complex numbers: <c>(a+bi) + (c+di) = (a+c) + (b+d)i</c>.
    /// </summary>
    public static Complex operator +(Complex c1, Complex c2)
    {
        return new Complex(c1._real + c2._real, c1._imaginary + c2._imaginary);
    }

    /// <summary>
    /// Subtracts two complex numbers: <c>(a+bi) - (c+di) = (a-c) + (b-d)i</c>.
    /// </summary>
    public static Complex operator -(Complex c1, Complex c2)
    {
        return new Complex(c1._real - c2._real, c1._imaginary - c2._imaginary);
    }

    /// <summary>
    /// Product of two complex numbers.
    /// Formula: <c>(a+bi)(c+di) = (ac-bd) + (ad+bc)i</c>.
    /// </summary>
    public static Complex operator *(Complex c1, Complex c2)
    {
        return new Complex(
            c1._real * c2._real - c1._imaginary * c2._imaginary,
            c1._real * c2._imaginary + c2._real * c1._imaginary);
    }

    /// <summary>
    /// Quotient of two complex numbers.
    /// Formula: <c>(a+bi)/(c+di) = [(ac+bd) + (bc-ad)i] / (c²+d²)</c>.
    /// </summary>
    /// <exception cref="DivideByZeroException">
    /// Thrown when <paramref name="c2"/> has both real and imaginary components equal to zero.
    /// </exception>
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
    /// <param name="c1">The first complex number.</param>
    /// <param name="c2">The second complex number.</param>
    /// <returns>The sum <c>c1 + c2</c>.</returns>
    public static Complex Add(Complex c1, Complex c2)
    {
        return c1 + c2;
    }

    /// <summary>
    /// Non-operator version of subtraction.
    /// </summary>
    /// <param name="c1">The first complex number.</param>
    /// <param name="c2">The second complex number.</param>
    /// <returns>The difference <c>c1 - c2</c>.</returns>
    public static Complex Subtract(Complex c1, Complex c2)
    {
        return c1 - c2;
    }

    /// <summary>
    /// Non-operator version of multiplication.
    /// </summary>
    /// <param name="c1">The first complex number.</param>
    /// <param name="c2">The second complex number.</param>
    /// <returns>The product <c>c1 * c2</c>.</returns>
    public static Complex Multiply(Complex c1, Complex c2)
    {
        return c1 * c2;
    }

    /// <summary>
    /// Non-operator version of division.
    /// </summary>
    /// <param name="c1">The dividend.</param>
    /// <param name="c2">The divisor. Must not be zero.</param>
    /// <returns>The quotient <c>c1 / c2</c>.</returns>
    /// <exception cref="DivideByZeroException">
    /// Thrown when <paramref name="c2"/> is zero.
    /// </exception>
    public static Complex Divide(Complex c1, Complex c2)
    {
        return c1 / c2;
    }

    /// <summary>
    /// Calculates the magnitude (absolute value) of a complex number.
    /// </summary>
    /// <param name="c">The complex number.</param>
    /// <returns>The magnitude: <c>√(real² + imaginary²)</c>.</returns>
    public static float Magnitude(Complex c)
    {
        return (float)Math.Sqrt(c._real * c._real + c._imaginary * c._imaginary);
    }
}