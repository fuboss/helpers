using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Steamworks;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Assets.Code.Tools
{
    static public class Util
    {
        public static T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            T comp = go.GetComponent<T>();

            if (comp == null)
            {
                Transform t = go.transform.parent;

                while (t != null && comp == null)
                {
                    comp = t.gameObject.GetComponent<T>();
                    t = t.parent;
                }
            }
            return comp;
        }

        /// <summary>
        /// Finds the specified component on the game object or one of its parents.
        /// </summary>
        public static T FindInParents<T>(Transform trans) where T : Component
        {
            if (trans == null) return null;
            return trans.GetComponentInParent<T>();

        }

        /// <summary>
        /// Add a new child game object.
        /// </summary>
        public static GameObject AddChild(GameObject parent, bool undo)
        {
            GameObject go = new GameObject();
#if UNITY_EDITOR
		if (undo) UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
            if (parent != null)
            {
                Transform t = go.transform;
                t.SetParent(parent.transform, false);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                go.layer = parent.layer;
            }
            return go;
        }

        /// <summary>
        /// Instantiate an object and add it to the specified parent.
        /// </summary>

        public static GameObject AddChild(GameObject parent, GameObject prefab)
        {
            GameObject go = Object.Instantiate(prefab);
            if (go != null && parent != null)
            {
                Transform t = go.transform;
                t.SetParent(parent.transform,true);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                go.layer = prefab.layer;
            }
            return go;
        }

        /// <summary>
        /// Instantiate an object and add it to the specified parent.
        /// </summary>

        public static T AddChild<T>(GameObject parent, T prefab) where T: MonoBehaviour
        {
            T go = Object.Instantiate(prefab);
            if (go != null && parent != null)
            {
                Transform t = go.transform;
                t.SetParent(parent.transform, true);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
                go.gameObject.layer = prefab.gameObject.layer;
                go.name = prefab.name;
            }
            return go;
        }

        /// <summary>
        /// Ensure that the angle is within -180 to 180 range.
        /// </summary>

        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerStepThrough]
        static public float WrapAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        public static IEnumerator UnscaledWait(float seconds)
        {
            var finishTime = Time.unscaledTime + seconds;
            while (Time.unscaledTime < finishTime)
                yield return null;
        }

        public static bool CustomApproximately(float val1, float val2, float epsilon = 0.001f)
        {
            return Mathf.Abs(Mathf.Abs(val1) - Mathf.Abs(val2)) < epsilon;
        }

        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }

        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }


        public static RectTransform AsRectTransform(this Transform transform)
        {
            return (RectTransform) transform;
        }

        public static void UpdateNavigationSettings(this Selectable target, Selectable up = null, Selectable down = null,
            Selectable left = null, Selectable right = null)
        {
            target.UpdateNavigationSettings(
                new UpdateNavigationData(up),
                new UpdateNavigationData(down),
                new UpdateNavigationData(left),
                new UpdateNavigationData(right));
        }

        public static void UpdateNavigationSettings(this Selectable target, UpdateNavigationData up, UpdateNavigationData down,
            UpdateNavigationData left, UpdateNavigationData right)
        {
            var nav = target.navigation;
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = (up.Target == null && up.LeftAsIs) ? nav.selectOnUp : up.Target;
            nav.selectOnDown = (down.Target == null && down.LeftAsIs) ? nav.selectOnDown : down.Target;
            nav.selectOnLeft = (left.Target == null && left.LeftAsIs) ? nav.selectOnLeft : left.Target;
            nav.selectOnRight = (right.Target == null && right.LeftAsIs) ? nav.selectOnRight : right.Target;
            target.navigation = nav;
        }

        public static IEnumerable<Selectable> AllNavigations(this Selectable target)
        {
            yield return target.navigation.selectOnDown;
            yield return target.navigation.selectOnUp;
            yield return target.navigation.selectOnLeft;
            yield return target.navigation.selectOnRight;
        }

        public static bool IsSelected(this Selectable target)
        {
            return EventSystem.current.currentSelectedGameObject != null && (target != null && EventSystem.current.currentSelectedGameObject.Equals(target.gameObject));
        }
    }

    public struct UpdateNavigationData
    {
        public Selectable Target { get; private set; }
        public bool LeftAsIs { get; private set; }

        public UpdateNavigationData(Selectable @new, bool leftAsIs=true) : this()
        {
            Target = @new;
            LeftAsIs = leftAsIs;
        }
    }
}
