using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ExampleTest
{
    [Test]
    public void TestFC_SceneManager()
    {
        var obj = Object.FindAnyObjectByType<MonoBehaviour>();
        Assert.NotNull(obj, "No MonoBehaviour found");
    }

    [Test]
    public void TestList()
    {
        var listOfStrings = new List<string> { "foo", "bar" };
        Assert.IsTrue(listOfStrings.Contains("foo"), "List does not contain element foo");
    }
}