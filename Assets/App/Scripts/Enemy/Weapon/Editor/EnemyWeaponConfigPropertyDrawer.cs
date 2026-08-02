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

            var verticalPosition = position.y;
            var width = position.width;

            var foldoutRect = new Rect(position.x, verticalPosition, width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            verticalPosition += lineHeight + spacing;

            var weaponTypeProp = property.FindPropertyRelative("_weaponType");
            var dropdownRect = new Rect(position.x, verticalPosition, width, lineHeight);
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(dropdownRect, weaponTypeProp);
            var typeChanged = EditorGUI.EndChangeCheck();
            var currentType = (WeaponType)weaponTypeProp.enumValueIndex;

            if (typeChanged)
            {
                ReplaceInstance(property, currentType);
            }

            verticalPosition += lineHeight + spacing;

            DrawField(ref verticalPosition, position, property, "_weaponId", lineHeight, spacing);
            DrawField(ref verticalPosition, position, property, "_damage", lineHeight, spacing);
            DrawField(ref verticalPosition, position, property, "_attackRange", lineHeight, spacing);
            DrawField(ref verticalPosition, position, property, "_attackCooldown", lineHeight, spacing);

            switch (currentType)
            {
                case WeaponType.Melee:
                    DrawField(ref verticalPosition, position, property, "_attackDuration", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_takeDamageTime", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_hitZoneSize", lineHeight, spacing);
                    break;
                case WeaponType.Range:
                    DrawField(ref verticalPosition, position, property, "_bulletId", lineHeight, spacing);
                    break;
                case WeaponType.Throwing:
                    DrawField(ref verticalPosition, position, property, "_throwAngle", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_minThrowForce", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_maxThrowForce", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_actionType", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_actionRadius", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_objectLifespan", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_gravityScale", lineHeight, spacing);
                    DrawField(ref verticalPosition, position, property, "_weaponPrefab", lineHeight, spacing);
                    break;
            }

            EditorGUI.EndProperty();
        }

        static void DrawField(ref float verticalPosition, Rect position, SerializedProperty property,
            string fieldName, float lineHeight, float spacing)
        {
            var relativeProperty = property.FindPropertyRelative(fieldName);
            if (relativeProperty != null)
            {
                EditorGUI.PropertyField(
                    new Rect(position.x, verticalPosition, position.width, lineHeight), relativeProperty);
                verticalPosition += lineHeight + spacing;
            }
        }

        static void ReplaceInstance(SerializedProperty property, WeaponType newType)
        {
            var oldDamage = property.FindPropertyRelative("_damage").floatValue;
            var oldRange = property.FindPropertyRelative("_attackRange").floatValue;
            var oldCooldown = property.FindPropertyRelative("_attackCooldown").floatValue;

            var oldAttackDuration = property.FindPropertyRelative("_attackDuration")?.floatValue ?? 0.5f;
            var oldTakeDamageTime = property.FindPropertyRelative("_takeDamageTime")?.floatValue ?? 0.5f;
            var oldHitZoneSize = property.FindPropertyRelative("_hitZoneSize")?.vector2Value ?? new Vector2(1.5f, 2f);

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

            var attackDurationProp = property.FindPropertyRelative("_attackDuration");
            if (attackDurationProp != null) attackDurationProp.floatValue = oldAttackDuration;
            var takeDamageTimeProp = property.FindPropertyRelative("_takeDamageTime");
            if (takeDamageTimeProp != null) takeDamageTimeProp.floatValue = oldTakeDamageTime;
            var hitZoneSizeProp = property.FindPropertyRelative("_hitZoneSize");
            if (hitZoneSizeProp != null) hitZoneSizeProp.vector2Value = oldHitZoneSize;

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
                WeaponType.Melee => 3,
                WeaponType.Range => 1,
                WeaponType.Throwing => 8,
                _ => 0,
            };

            int fieldLines = 5 + extraLines;
            return (fieldLines + 1) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        }
    }
}
