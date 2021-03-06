﻿
using System;
////using Bonsai.Core;
using UnityEngine;

namespace Saro.BT.Designer
{
  public static class EditorChangeNodeType
  {
    public static void ChangeType(BonsaiNode node, Type newType)
    {
      var newBehaviour = ScriptableObject.CreateInstance(newType) as BTNode;
      node.SetBehaviour(newBehaviour, NodeIcon(newType));
    }

    private static Texture NodeIcon(Type behaviourType)
    {
      var prop = BonsaiEditor.GetNodeTypeProperties(behaviourType);
      return BonsaiPreferences.Texture(prop.texName);
    }
  }
}
