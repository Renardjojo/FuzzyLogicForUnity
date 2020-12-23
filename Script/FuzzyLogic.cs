using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;
using GUIContent = UnityEngine.GUIContent;
using PropertyAttribute = UnityEngine.PropertyAttribute;
using Rect = UnityEngine.Rect;

public class Point2D : IComparable
{
    public float X { get; set; }
    public float Y { get; set; }

    public Point2D(float newX, float newY)
    {
        X = newX;
        Y = newY;
    }

    public int CompareTo(object obj)
    {
        return (int) (X - ((Point2D) obj).X);
    }

    public override string ToString()
    {
        return "x : " + X + "; y : " + Y;
    }
}

public class FuzzySet
{
    protected List<Point2D> m_points;
    protected float Min { get; set; }
    protected float Max { get; set; }

    FuzzySet(float newMin, float newMax)
    {
        m_points = new List<Point2D>();
        Min = newMin;
        Max = newMax;
    }

    public void Add(Point2D newPoint)
    {
        m_points.Add(newPoint);
        m_points.Sort();
    }
    
    public void Add(float newX, float newY)
    {
        Point2D newPoint = new Point2D(newX, newY);
        Add(newPoint);
    }

    public override string ToString()
    {
        String result = "[" + Min + "; " + Max + "]";

        foreach (Point2D pt in m_points)
        {
            result += pt.ToString();
        }
        return result;
    }

    public static Boolean operator !=(FuzzySet lhs, FuzzySet rhs)
    {
        return !(lhs == rhs);
    }
    
    public static Boolean operator ==(FuzzySet lhs, FuzzySet rhs)
    {
        return lhs.ToString().Equals(rhs.ToString());
    }

    public static FuzzySet operator *(FuzzySet fs, float value)
    {
        FuzzySet newFuzzySet = new FuzzySet(fs.Min, fs.Max);
        foreach (Point2D pt in fs.m_points)
        {
            newFuzzySet.Add(pt.X, pt.Y * value);
        }

        return newFuzzySet;
    }
    
    public static FuzzySet operator !(FuzzySet fs)
    {
        FuzzySet newFuzzySet = new FuzzySet(fs.Min, fs.Max);
        foreach (Point2D pt in fs.m_points)
        {
            newFuzzySet.Add(pt.X, 1f - pt.Y);
        }

        return newFuzzySet;
    }

    public float DegreeAtValue(float xValue)
    {
        if (ValueOutOfBound(xValue))
        {
            return 0;
        }
        else
        {
            return GetValueFromInterpolation(xValue);
        }
    }

    private bool ValueOutOfBound(float xValue)
    {
        return (xValue < Min || xValue > Max);
    }

    private float GetValueFromInterpolation(float xValue)
    {
        Point2D before = m_points.LastOrDefault(pt => pt.X <= xValue);
        Point2D after = m_points.LastOrDefault(pt => pt.X >= xValue);
        
        if (before.Equals(after))
        {
            return before.Y;
        }
        else
        {
            return InterpolationBetweenTwoPoints(xValue, before, after);
        }
    }

    private float InterpolationBetweenTwoPoints(float xValue, Point2D before, Point2D after)
    {
        return (((before.Y - after.Y) * (after.X - xValue) / (after.X - before.X)) + after.Y);
    }
}

[System.Serializable]
public class FuzzyLogic
{
    //[NamedArrayAttribute (new string[] {})]
    public List<AnimationCurve> m_fuzzySet;

    void Init()
    {}
}

public class NamedArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public NamedArrayAttribute(string[] names) { this.names = names; }
}
 
[CustomPropertyDrawer (typeof(NamedArrayAttribute))]public class NamedArrayDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        try {
            int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
            EditorGUI.CurveField(Rect.MinMaxRect(0, 0, 10, 100), property, Color.blue, Rect.MinMaxRect(0, 0, 1, 1),  new GUIContent(((NamedArrayAttribute)attribute).names[pos]));
        } catch {
            EditorGUI.CurveField(rect, property, Color.blue, Rect.MinMaxRect(0, 0, 1, 1), label);
        }
    }
}

