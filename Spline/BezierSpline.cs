using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Assets.Code.Tools.Spline
{
    public class BezierSpline : MonoBehaviour
    {
        public Color DebugTint = Color.white;
        public bool DebugDraw = true;

        public bool Loop
        {
            get { return _loop; }
            set
            {
                _loop = value;
                if (value)
                {
                    _modes[_modes.Count - 1] = _modes[0];
                    SetControlPoint(0, _points[0]);
                }
            }
        }

        public int ControlPointCount
        {
            get { return _points.Count; }
        }

        public int CurveCount
        {
            get { return (_points.Count - 1) / 3; }
        }

        public Vector3 GetControlPoint(int index)
        {
            return _points[index];
        }

        public void SetControlPoint(int index, Vector3 point)
        {
            if (index%3 == 0)
            {
                Vector3 delta = point - _points[index];
                if (_loop)
                {
                    if (index == 0)
                    {
                        _points[1] += delta;
                        _points[_points.Count - 2] += delta;
                        _points[_points.Count - 1] = point;
                    }
                    else if (index == _points.Count - 1)
                    {
                        _points[0] = point;
                        _points[1] += delta;
                        _points[index - 1] += delta;
                    }
                    else
                    {
                        _points[index - 1] += delta;
                        _points[index + 1] += delta;
                    }
                }
                else
                {
                    if (index > 0)
                        _points[index - 1] += delta;
                    if (index + 1 < _points.Count)
                        _points[index + 1] += delta;
                }
            }
            _points[index] = point;
            EnforceMode(index);
        }

        public BezierControlPointMode GetControlPointMode(int index)
        {
            return _modes[(index + 1)/3];
        }

        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {
            int modeIndex = (index + 1)/3;
            _modes[modeIndex] = mode;
            if (_loop)
            {
                if (modeIndex == 0)
                    _modes[_modes.Count - 1] = mode;
                else if (modeIndex == _modes.Count - 1)
                    _modes[0] = mode;
            }
            EnforceMode(index);
        }

        public Vector3 GetPoint(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = _points.Count - 4;
            }
            else
            {
                t = Mathf.Clamp01(t)*CurveCount;
                i = (int) t;
                t -= i;
                i *= 3;
            }
            return
                transform.TransformPoint(Bezier.GetPoint(_points[i], _points[i + 1], _points[i + 2], _points[i + 3], t));
        }

        public Vector3 GetVelocity(float t)
        {
            int i;
            if (t >= 1f)
            {
                t = 1f;
                i = _points.Count - 4;
            }
            else
            {
                t = Mathf.Clamp01(t)*CurveCount;
                i = (int) t;
                t -= i;
                i *= 3;
            }
            return
                transform.TransformPoint(Bezier.GetFirstDerivative(_points[i], _points[i + 1], _points[i + 2],
                    _points[i + 3], t)) - transform.position;
        }

        public Vector3 GetDirection(float t)
        {
            return GetVelocity(t).normalized;
        }

        public void AddCurve()
        {
            Vector3 point = _points[_points.Count - 1];
            _points.AddRange(new[] { new Vector3(), new Vector3(), new Vector3() });

            point.x += 1f;
            _points[_points.Count - 3] = point;
            point.x += 1f;
            _points[_points.Count - 2] = point;
            point.x += 1f;
            _points[_points.Count - 1] = point;

            
            _modes.Add(_modes[_modes.Count - 1]);
            EnforceMode(_points.Count - 4);

            if (_loop)
            {
                _points[_points.Count - 1] = _points[0];
                _modes[_modes.Count - 1] = _modes[0];
                EnforceMode(0);
            }
        }

        public void RemoveLastCurve()
        {
            if (_points.Count < 6) return;

            _points.RemoveRange(_points.Count - 3, 3);
            
            _modes.RemoveAt(_modes.Count-1);
            EnforceMode(_points.Count - 1);
        }

        public void Reset()
        {
            _points.AddRange(new []
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(2f, 0f, 0f),
                new Vector3(3f, 0f, 0f),
                new Vector3(4f, 0f, 0f)
            });
            _modes.AddRange(new[]
            {
                BezierControlPointMode.Free,
                BezierControlPointMode.Free
            });
        }

        [SerializeField] private List<Vector3> _points = new List<Vector3>();

        [SerializeField] private List<BezierControlPointMode> _modes = new List<BezierControlPointMode>();

        [SerializeField] private bool _loop;

        private void EnforceMode(int index)
        {
            int modeIndex = (index + 1)/3;
            BezierControlPointMode mode = _modes[modeIndex];
            if (mode == BezierControlPointMode.Free || !_loop && (modeIndex == 0 || modeIndex == _modes.Count - 1))
                return;

            int middleIndex = modeIndex*3;
            int fixedIndex, enforcedIndex;
            if (index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                if (fixedIndex < 0)
                    fixedIndex = _points.Count - 2;
                enforcedIndex = middleIndex + 1;
                if (enforcedIndex >= _points.Count)
                    enforcedIndex = 1;
            }
            else
            {
                fixedIndex = middleIndex + 1;
                if (fixedIndex >= _points.Count)
                    fixedIndex = 1;
                enforcedIndex = middleIndex - 1;
                if (enforcedIndex < 0)
                    enforcedIndex = _points.Count - 2;
            }

            Vector3 middle = _points[middleIndex];
            Vector3 enforcedTangent = middle - _points[fixedIndex];
            if (mode == BezierControlPointMode.Aligned)
                enforcedTangent = enforcedTangent.normalized*Vector3.Distance(middle, _points[enforcedIndex]);
            _points[enforcedIndex] = middle + enforcedTangent;
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_points == null || _points.Count == 0 || !DebugDraw) return;

            float progress = 0;
            while (progress < 1)
            {
                progress += 0.2f / _points.Count;
                Gizmos.color = DebugTint;
                Gizmos.DrawSphere(GetPoint(progress), 0.03f);
            }
        }
#endif
    }

}