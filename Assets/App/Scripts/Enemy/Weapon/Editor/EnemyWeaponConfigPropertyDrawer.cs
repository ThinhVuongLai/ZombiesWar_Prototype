using UnityEditor;
using UnityEngine;
using App.Enemy.Weapon;

namespace App.Enemy.Weapon.Editor
{
    [CustomPropertyDrawer(typeof(EnemyWeaponConfig), true)]
    public class EnemyWeaponConfigPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = EditorGUIUtility.standardVerticalSpacing;

            if (property.managedReferenceValue == null)
            {
                var nullRect = new Rect(position.x, position.y, position.width, lineHeight);
                property.isExpanded = EditorGUI.Foldout(nullRect, property.isExpanded, label, true);
                if (property.isExpanded)
                {
                    nullRect.y += lineHeight + spacing;
                    if (GUI.Button(nullRect, "Create New (Melee)"))
                    {
                        property.managedReferenceValue = new EnemyMeleeWeaponConfig();
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUI.EndProperty();
                return;
            }

            var y = position.y;
            var width = position.width;

            var foldoutRect = new Rect(position.x, y, width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            y += lineHeight + spacing;

            var weaponTypeProp = property.FindPropertyRelative("_weaponType");
            var dropdownRect = new Rect(position.x, y, width, lineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(dropdownRect, weaponTypeProp);
            var typeChanged = EditorGUI.EndChangeCheck();
            var currentType = (WeaponType)weaponTypeProp.enumValueIndex;

            if (typeChanged)
            {
                ReplaceInstance(property, currentType);
            }

            y += lineHeight + spacing;

            DrawField(ref y, position, property, "_damage", lineHeight, spacing);
            DrawField(ref y, position, property, "_attackRange", lineHeight, spacing);
            DrawField(ref y, position, property, "_attackCooldown", lineHeight, spacing);

            switch (currentType)
            {
                case WeaponType.Range:
                    DrawField(ref y, position, property, "_bulletId", lineHeight, spacing);
                    break;
                case WeaponType.Throwing:
                    DrawField(ref y, position, property, "_throwAngle", lineHeight, spacing);
                    DrawField(ref y, position, property, "_minThrowForce", lineHeight, spacing);
                    DrawField(ref y, position, property, "_maxThrowForce", lineHeight, spacing);
                    DrawField(ref y, position, property, "_actionType", lineHeight, spacing);
                    DrawField(ref y, position, property, "_actionRadius", lineHeight, spacing);
                    DrawField(ref y, position, property, "_objectLifespan", lineHeight, spacing);
                    DrawField(ref y, position, property, "_gravityScale", lineHeight, spacing);
                    DrawField(ref y, position, property, "_objectPrefab", lineHeight, spacing);
                    break;
            }

            EditorGUI.EndProperty();
        }

        static void DrawField(ref float y, Rect position, SerializedProperty property,
            string fieldName, float lineHeight, float spacing)
        {
            var prop = property.FindPropertyRelative(fieldName);
            if (prop != null)
            {
                EditorGUI.PropertyField(
                    new Rect(position.x, y, position.width, lineHeight), prop);
                y += lineHeight + spacing;
            }
        }

        static void ReplaceInstance(SerializedProperty property, WeaponType newType)
        {
            var oldDamage = property.FindPropertyRelative("_damage").floatValue;
            var oldRange = property.FindPropertyRelative("_attackRange").floatValue;
            var oldCooldown = property.FindPropertyRelative("_attackCooldown").floatValue;

            EnemyWeaponConfig newInstance = newType switch
            {
                WeaponType.Melee => new EnemyMeleeWeaponConfig(),
                WeaponType.Range => new EnemyRangedWeaponConfig(),
                WeaponType.Throwing => new EnemyThrowWeaponConfig(),
                _ => new EnemyMeleeWeaponConfig(),
            };

            property.managedReferenceValue = newInstance;

            property.FindPropertyRelative("_weaponType").enumValueIndex = (int)newType;
            property.FindPropertyRelative("_damage").floatValue = oldDamage;
            property.FindPropertyRelative("_attackRange").floatValue = oldRange;
            property.FindPropertyRelative("_attackCooldown").floatValue = oldCooldown;

            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            if (property.managedReferenceValue == null)
                return 2 * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);

            var weaponTypeProp = property.FindPropertyRelative("_weaponType");
            var type = weaponTypeProp != null
                ? (WeaponType)weaponTypeProp.enumValueIndex
                : WeaponType.Melee;

            int extraLines = type switch
            {
                WeaponType.Range => 1,
                WeaponType.Throwing => 8,
                _ => 0,
            };

            int fieldLines = 4 + extraLines;
            return (fieldLines + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
    }
}
