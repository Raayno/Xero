using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class AlternatingTurnSelectorTests
{
    [Test]
    public void NextTurn_InitializesAndUpdatesAlternatingTimeline()
    {
        var selectorObject = new GameObject("Selector");
        var selectorType = FindType("AlternatingTurnSelector");
        Assert.That(selectorType, Is.Not.Null, "Could not locate AlternatingTurnSelector at runtime.");

        var selector = selectorObject.AddComponent(selectorType);
        SetProtectedField(selector, selectorType.BaseType, "foresightLength", 10);

        var playerType = FindType("PlayerParticipant");
        var enemyType = FindType("EnemyParticipant");

        Assert.That(playerType, Is.Not.Null, "Could not locate PlayerParticipant at runtime.");
        Assert.That(enemyType, Is.Not.Null, "Could not locate EnemyParticipant at runtime.");

        var players = CreateParticipantList(playerType);
        var p1 = CreatePlayer("P1", playerType, players);
        var p2 = CreatePlayer("P2", playerType, players);
        var p3 = CreatePlayer("P3", playerType, players);

        var enemies = CreateParticipantList(enemyType);
        var e1 = CreateEnemy("E1", enemyType, enemies);
        var e2 = CreateEnemy("E2", enemyType, enemies);
        var e3 = CreateEnemy("E3", enemyType, enemies);
        var e4 = CreateEnemy("E4", enemyType, enemies);

        try
        {
            InvokeNextTurn(selectorType, selector, players, enemies);

            Assert.That(
                GetTimelineNames(selectorType, selector),
                Is.EqualTo(new[] { "P1", "E1", "P2", "E2", "P3", "E3", "P1", "E4", "P2", "E1" }));

            enemies.Remove(e2);
            players.Remove(p1);

            InvokeNextTurn(selectorType, selector, players, enemies);

            Assert.That(
                GetTimelineNames(selectorType, selector),
                Is.EqualTo(new[] { "E1", "P2", "P3", "E3", "E4", "P2", "E1", "P3", "E3", "P2" }));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(selectorObject);
            DestroyParticipant(p1);
            DestroyParticipant(p2);
            DestroyParticipant(p3);
            DestroyParticipant(e1);
            DestroyParticipant(e2);
            DestroyParticipant(e3);
            DestroyParticipant(e4);
        }
    }

    private static Type FindType(string typeName)
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(assembly => assembly.GetType(typeName))
            .FirstOrDefault(type => type != null);
    }

    private static IList CreateParticipantList(Type participantType)
    {
        return (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(participantType));
    }

    private static Component CreatePlayer(string name, Type participantType, IList participants)
    {
        return CreateParticipant(name, participantType, participants, true);
    }

    private static Component CreateEnemy(string name, Type participantType, IList participants)
    {
        return CreateParticipant(name, participantType, participants, false);
    }

    private static Component CreateParticipant(string name, Type participantType, IList participants, bool isPlayerTeam)
    {
        var gameObject = new GameObject(name);
        var participant = gameObject.AddComponent(participantType);
        participantType.GetField("IsPlayerTeam", BindingFlags.Instance | BindingFlags.Public).SetValue(participant, isPlayerTeam);
        participants.Add(participant);
        return participant;
    }

    private static void SetProtectedField(object target, Type declaringType, string fieldName, object value)
    {
        declaringType
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(target, value);
    }

    private static void InvokeNextTurn(Type selectorType, object selector, object players, object enemies)
    {
        selectorType.GetMethod("NextTurn").Invoke(selector, new[] { players, enemies });
    }

    private static IEnumerable<string> GetTimelineNames(Type selectorType, object selector)
    {
        var timeline = (IEnumerable)selectorType
            .BaseType
            .GetProperty("TurnTimeline", BindingFlags.Instance | BindingFlags.Public)
            .GetValue(selector);

        return timeline.Cast<Component>().Select(participant => participant.gameObject.name);
    }

    private static void DestroyParticipant(Component participant)
    {
        if (participant != null)
        {
            UnityEngine.Object.DestroyImmediate(participant.gameObject);
        }
    }
}