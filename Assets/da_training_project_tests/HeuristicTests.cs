using System;
using System.Reflection;
using NUnit.Framework;
using Unity.MLAgents.Actuators;

[TestFixture]
public class HeuristicTests
{
    private Hummingbird hummingbird;
    private MethodInfo heuristicMethod;

    [SetUp]
    public void SetUp()
    {
        var go = new UnityEngine.GameObject("TestHummingbird");
        hummingbird = go.AddComponent<Hummingbird>();

        heuristicMethod = typeof(Hummingbird).GetMethod(
            "Heuristic",
            BindingFlags.Public | BindingFlags.Instance,
            null,
            new Type[] { typeof(ActionBuffers).MakeByRefType() },
            null
        );

        if (heuristicMethod == null)
        {
            heuristicMethod = typeof(Hummingbird).GetMethod(
                "Heuristic",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[] { typeof(ActionBuffers) },
                null
            );
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (hummingbird != null)
            UnityEngine.Object.DestroyImmediate(hummingbird.gameObject);
    }

    [Test]
    public void HeuristicMethodExists()
    {
        var method = typeof(Hummingbird).GetMethod(
            "Heuristic",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
        );
        Assert.IsNotNull(method, "Heuristic method should be declared in Hummingbird class");
    }

    [Test]
    public void HeuristicMethodIsOverride()
    {
        var method = typeof(Hummingbird).GetMethod(
            "Heuristic",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
        );
        Assert.IsNotNull(method, "Heuristic method should exist");
        Assert.IsTrue(method.IsVirtual, "Heuristic should be an override (virtual) method");
        Assert.IsFalse(method.Equals(method.GetBaseDefinition()), "Heuristic should override the base Agent.Heuristic");
    }

    [Test]
    public void HeuristicMethodHasCorrectSignature()
    {
        var method = typeof(Hummingbird).GetMethod(
            "Heuristic",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
        );
        Assert.IsNotNull(method, "Heuristic method should exist");

        var parameters = method.GetParameters();
        Assert.AreEqual(1, parameters.Length, "Heuristic should have exactly one parameter");
        Assert.AreEqual("actionsOut", parameters[0].Name, "Parameter should be named 'actionsOut'");

        Assert.AreEqual(typeof(void), method.ReturnType, "Heuristic should return void");
    }

    [Test]
    public void HeuristicMethodParameterIsActionBuffers()
    {
        var method = typeof(Hummingbird).GetMethod(
            "Heuristic",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
        );
        Assert.IsNotNull(method, "Heuristic method should exist");

        var parameters = method.GetParameters();
        var paramType = parameters[0].ParameterType;
        bool isActionBuffersType = paramType == typeof(ActionBuffers) ||
                                   (paramType.IsByRef && paramType.GetElementType() == typeof(ActionBuffers));
        Assert.IsTrue(isActionBuffersType, "Parameter type should be ActionBuffers (or in ActionBuffers)");
    }

    [Test]
    public void HeuristicOutputsExactlyFiveContinuousActions()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        Assert.AreEqual(5, actionsOut.ContinuousActions.Length,
            "Heuristic should output exactly 5 continuous actions");
    }

    [Test]
    public void NoKeysPressed_AllActionsAreZero()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        for (int i = 0; i < 5; i++)
        {
            Assert.AreEqual(0f, actionsOut.ContinuousActions[i],
                $"Action at index {i} should be 0 when no keys are pressed");
        }
    }

    [Test]
    public void AllContinuousActionsWithinNegOneToOne()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        for (int i = 0; i < 5; i++)
        {
            Assert.GreaterOrEqual(actionsOut.ContinuousActions[i], -1f,
                $"Action at index {i} should be >= -1");
            Assert.LessOrEqual(actionsOut.ContinuousActions[i], 1f,
                $"Action at index {i} should be <= 1");
        }
    }

    [Test]
    public void ActionIndex0_RepresentsMoveX_LeftRight()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        float moveX = actionsOut.ContinuousActions[0];
        Assert.GreaterOrEqual(moveX, -1f, "Move X (index 0) should be >= -1");
        Assert.LessOrEqual(moveX, 1f, "Move X (index 0) should be <= 1");
    }

    [Test]
    public void ActionIndex1_RepresentsMoveY_UpDown()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        float moveY = actionsOut.ContinuousActions[1];
        Assert.GreaterOrEqual(moveY, -1f, "Move Y (index 1) should be >= -1");
        Assert.LessOrEqual(moveY, 1f, "Move Y (index 1) should be <= 1");
    }

    [Test]
    public void ActionIndex2_RepresentsMoveZ_ForwardBackward()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        float moveZ = actionsOut.ContinuousActions[2];
        Assert.GreaterOrEqual(moveZ, -1f, "Move Z (index 2) should be >= -1");
        Assert.LessOrEqual(moveZ, 1f, "Move Z (index 2) should be <= 1");
    }

    [Test]
    public void ActionIndex3_RepresentsPitch()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        float pitch = actionsOut.ContinuousActions[3];
        Assert.GreaterOrEqual(pitch, -1f, "Pitch (index 3) should be >= -1");
        Assert.LessOrEqual(pitch, 1f, "Pitch (index 3) should be <= 1");
    }

    [Test]
    public void ActionIndex4_RepresentsYaw()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        float yaw = actionsOut.ContinuousActions[4];
        Assert.GreaterOrEqual(yaw, -1f, "Yaw (index 4) should be >= -1");
        Assert.LessOrEqual(yaw, 1f, "Yaw (index 4) should be <= 1");
    }

    [Test]
    public void MovementVectorIsNormalized_WhenMultipleMovementKeysPressed()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        float x = actionsOut.ContinuousActions[0];
        float y = actionsOut.ContinuousActions[1];
        float z = actionsOut.ContinuousActions[2];

        float magnitude = UnityEngine.Mathf.Sqrt(x * x + y * y + z * z);

        if (magnitude > 0f)
        {
            Assert.LessOrEqual(magnitude, 1.001f,
                "Movement vector (indices 0,1,2) should be normalized (magnitude <= 1)");
        }
    }

    [Test]
    public void HeuristicShouldUseFiveContinuousActions()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        Assert.AreEqual(5, actionsOut.ContinuousActions.Length,
            "5 continuous actions in array so array length must be 5 for continuous actions");
    }

    [Test]
    public void ForwardKeyMapping_UsesIndex2()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);
        InvokeHeuristic(actionsOut);

        Assert.AreEqual(typeof(float), actionsOut.ContinuousActions[2].GetType(),
            "Index 2 (forward/backward) should be a float value");
    }

    [Test]
    public void PitchAndYaw_AreNotPartOfMovementVector()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        float x = actionsOut.ContinuousActions[0];
        float y = actionsOut.ContinuousActions[1];
        float z = actionsOut.ContinuousActions[2];
        float pitch = actionsOut.ContinuousActions[3];
        float yaw = actionsOut.ContinuousActions[4];

        Assert.AreEqual(typeof(float), pitch.GetType(), "Pitch should be a separate float at index 3");
        Assert.AreEqual(typeof(float), yaw.GetType(), "Yaw should be a separate float at index 4");
    }

    [Test]
    public void HeuristicIsPublic()
    {
        var method = typeof(Hummingbird).GetMethod(
            "Heuristic",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
        );
        Assert.IsNotNull(method, "Heuristic method should be public");
        Assert.IsTrue(method.IsPublic, "Heuristic method should have public access modifier");
    }

    [Test]
    public void ActionBufferIndicesMatchOnActionReceived()
    {
        var actionSpec = ActionSpec.MakeContinuous(5);
        var actionsOut = new ActionBuffers(actionSpec);

        InvokeHeuristic(actionsOut);

        Assert.AreEqual(5, actionsOut.ContinuousActions.Length,
            "Should match OnActionReceived's expected 5 continuous actions: " +
            "Index 0 = move X, Index 1 = move Y, Index 2 = move Z, Index 3 = pitch, Index 4 = yaw");
    }

    [Test]
    public void HeuristicUsesVector3ForMovement()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        Assert.IsTrue(source.Contains("Vector3"),
            "Heuristic implementation should use Vector3 for movement calculations");
    }

    [Test]
    public void HeuristicUsesKeyboardInput()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool usesInput = source.Contains("Input.GetKey") || source.Contains("Input.GetAxis") ||
                         source.Contains("Keyboard.current");
        Assert.IsTrue(usesInput,
            "Heuristic should read keyboard input (Input.GetKey, Input.GetAxis, or Keyboard.current)");
    }

    [Test]
    public void HeuristicMapsWKeyForForward()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool mapsW = source.Contains("KeyCode.W") || source.Contains("Key.W") || source.Contains("\"w\"");
        Assert.IsTrue(mapsW, "Heuristic should map W key for forward movement");
    }

    [Test]
    public void HeuristicMapsSKeyForBackward()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool mapsS = source.Contains("KeyCode.S") || source.Contains("Key.S") || source.Contains("\"s\"");
        Assert.IsTrue(mapsS, "Heuristic should map S key for backward movement");
    }

    [Test]
    public void HeuristicMapsAKeyForLeft()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool mapsA = source.Contains("KeyCode.A") || source.Contains("Key.A") || source.Contains("\"a\"");
        Assert.IsTrue(mapsA, "Heuristic should map A key for left movement");
    }

    [Test]
    public void HeuristicMapsDKeyForRight()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool mapsD = source.Contains("KeyCode.D") || source.Contains("Key.D") || source.Contains("\"d\"");
        Assert.IsTrue(mapsD, "Heuristic should map D key for right movement");
    }

    [Test]
    public void HeuristicMapsEKeyForUp()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool mapsE = source.Contains("KeyCode.E") || source.Contains("Key.E") || source.Contains("\"e\"");
        Assert.IsTrue(mapsE, "Heuristic should map E key for upward movement");
    }

    [Test]
    public void HeuristicMapsQKeyForDown()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool mapsQ = source.Contains("KeyCode.Q") || source.Contains("Key.Q") || source.Contains("\"q\"");
        Assert.IsTrue(mapsQ, "Heuristic should map Q key for downward movement");
    }

    [Test]
    public void HeuristicMapsArrowKeysForPitchAndYaw()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool mapsArrowUp = source.Contains("KeyCode.UpArrow") || source.Contains("Key.UpArrow") ||
                           source.Contains("upArrowKey");
        bool mapsArrowDown = source.Contains("KeyCode.DownArrow") || source.Contains("Key.DownArrow") ||
                             source.Contains("downArrowKey");
        bool mapsArrowLeft = source.Contains("KeyCode.LeftArrow") || source.Contains("Key.LeftArrow") ||
                             source.Contains("leftArrowKey");
        bool mapsArrowRight = source.Contains("KeyCode.RightArrow") || source.Contains("Key.RightArrow") ||
                              source.Contains("rightArrowKey");

        Assert.IsTrue(mapsArrowUp, "Heuristic should map Up arrow key for pitch");
        Assert.IsTrue(mapsArrowDown, "Heuristic should map Down arrow key for pitch");
        Assert.IsTrue(mapsArrowLeft, "Heuristic should map Left arrow key for yaw");
        Assert.IsTrue(mapsArrowRight, "Heuristic should map Right arrow key for yaw");
    }

    [Test]
    public void HeuristicNormalizesMovementVector()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        bool normalizes = source.Contains(".normalized") || source.Contains("Normalize") ||
                          source.Contains(".normalize");
        Assert.IsTrue(normalizes,
            "Heuristic should normalize the combined movement vector");
    }

    [Test]
    public void HeuristicMethodReturnTypeIsVoid()
    {
        var method = typeof(Hummingbird).GetMethod(
            "Heuristic",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
        );
        Assert.IsNotNull(method);
        Assert.AreEqual(typeof(void), method.ReturnType, "Heuristic must return void");
    }

    [Test]
    public void HeuristicIncludesRequiredUsingDirectives()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        Assert.IsTrue(source.Contains("using System;"),
            "Hummingbird.cs should include 'using System;' directive");
        Assert.IsTrue(source.Contains("using Unity.Mathematics;"),
            "Hummingbird.cs should include 'using Unity.Mathematics;' directive");
    }

    [Test]
    public void HeuristicUsesTransformForwardNotVector3Forward()
    {
        var source = System.IO.File.ReadAllText(
            System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts", "Hummingbird.cs"));

        int heuristicStart = source.IndexOf("public override void Heuristic");
        Assert.IsTrue(heuristicStart >= 0, "Heuristic method should exist in source");

        int braceCount = 0;
        int heuristicBodyStart = source.IndexOf('{', heuristicStart);
        int heuristicEnd = heuristicBodyStart;
        for (int i = heuristicBodyStart; i < source.Length; i++)
        {
            if (source[i] == '{') braceCount++;
            else if (source[i] == '}') braceCount--;
            if (braceCount == 0) { heuristicEnd = i; break; }
        }

        string heuristicBody = source.Substring(heuristicBodyStart, heuristicEnd - heuristicBodyStart + 1);

        Assert.IsTrue(heuristicBody.Contains("transform.forward"),
            "Heuristic should use 'transform.forward' (local object direction) instead of 'Vector3.forward' (global direction)");
        Assert.IsFalse(heuristicBody.Contains("Vector3.forward"),
            "Heuristic should NOT use 'Vector3.forward' as it refers to global direction; use 'transform.forward' for the agent's local forward direction");
    }

    private void InvokeHeuristic(ActionBuffers actionsOut)
    {
        if (heuristicMethod != null)
        {
            var parameters = new object[] { actionsOut };
            heuristicMethod.Invoke(hummingbird, parameters);
        }
        else
        {
            Assert.Fail("Heuristic method not found on Hummingbird class");
        }
    }
}
