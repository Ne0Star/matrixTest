using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class MatrixxLoader : MonoBehaviour
{
    [SerializeField] private string matchingPath;
    [SerializeField] private string modelPath;
    [SerializeField] private string spacePath;
    [SerializeField] private Matrix4x4[] previewModelMatrix;
    [SerializeField] private Matrix4x4[] previewSpaceMatrix;
    [SerializeField] private Matrix4x4[] matchingMatrices; // 

    private void SaveMatchingMatricesToJson(string path, Matrix4x4[] matchingMatrices)
    {
        MatrixContainer container = new MatrixContainer();
        MatrixData[] datas = new MatrixData[matchingMatrices.Length];
        for (int i = 0; i < matchingMatrices.Length; i++)
        {
            Matrix4x4 matrix = matchingMatrices[i];
            MatrixData data = new MatrixData();
            data.m00 = matrix.m00;
            data.m01 = matrix.m01;
            data.m02 = matrix.m02;
            data.m03 = matrix.m03;
            data.m10 = matrix.m10;
            data.m11 = matrix.m11;
            data.m12 = matrix.m12;
            data.m13 = matrix.m13;
            data.m20 = matrix.m20;
            data.m21 = matrix.m21;
            data.m22 = matrix.m22;
            data.m23 = matrix.m23;
            data.m30 = matrix.m30;
            data.m31 = matrix.m31;
            data.m32 = matrix.m32;
            data.m33 = matrix.m33;
            datas[i] = data;
        }
        container.datas = datas;
        string jsonText = JsonUtility.ToJson(container);
        System.IO.File.WriteAllText(path, jsonText);
    }

    private Matrix4x4[] FindMatchingMatrices(Matrix4x4[] modelMatrices, Matrix4x4[] spaceMatrices)
    {
        List<Matrix4x4> result = new();
        foreach (Matrix4x4 modelMatrix in modelMatrices)
        {
            foreach (Matrix4x4 spaceMatrix in spaceMatrices)
            {
                if (IsMatchingMatrix(modelMatrix, spaceMatrix))
                {
                    result.Add(modelMatrix);
                    break;
                }
            }
        }
        return result.ToArray();
    }
    private bool IsMatchingMatrix(Matrix4x4 modelMatrix, Matrix4x4 spaceMatrix)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (Mathf.Abs(modelMatrix[i, j] - spaceMatrix[i, j]) > float.Epsilon)
                {
                    return false;
                }
            }
        }
        return true;
    }
#if UNITY_EDITOR
    [SerializeField] private bool updateData;
#endif
    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (updateData)
        {
            string modelJson = GetJson(modelPath);
            string spaceJson = GetJson(spacePath);
            previewModelMatrix = ToMatrixArray(JsonToMatrixContainer(modelJson));
            previewSpaceMatrix = ToMatrixArray(JsonToMatrixContainer(spaceJson));

            //Матрицы, найденные с помощью алгоритма - зеленый.
            //Матрицы модели, которые не совпадают с матрицами пространства - красный.
            //Матрицы пространства - синий.

            matchingMatrices = FindMatchingMatrices(previewModelMatrix, previewSpaceMatrix);
            List<Matrix4x4> result = new List<Matrix4x4>();
            Gizmos.color = Color.green;
            foreach (Matrix4x4 matrix in matchingMatrices)
            {
                DrawMatrixGizmo(matrix);
               result.Add(matrix);
            }
            Gizmos.color = Color.red;
            foreach (Matrix4x4 matrix in previewModelMatrix)
            {
                if (!matchingMatrices.Contains(matrix))
                {
                    DrawMatrixGizmo(matrix);
                }
            }
            Gizmos.color = Color.blue;
            foreach (Matrix4x4 matrix in previewSpaceMatrix)
            {
                DrawMatrixGizmo(matrix);
            }

            matchingMatrices = result.ToArray();
            SaveMatchingMatricesToJson(matchingPath, matchingMatrices);

            updateData = false;
        }
#endif
    }
    private void DrawMatrixGizmo(Matrix4x4 matrix)
    {
        Vector3 position = matrix.GetColumn(3);
        Quaternion rotation = Quaternion.LookRotation(matrix.GetColumn(2), matrix.GetColumn(1));
        Gizmos.DrawWireCube(position, new Vector3(0.1f, 0.1f, 0.1f));
        Gizmos.DrawWireSphere(position, 0.05f);
        Gizmos.DrawRay(position, rotation * Vector3.forward * 0.1f);
        Gizmos.DrawRay(position, rotation * Vector3.up * 0.1f);
        Gizmos.DrawRay(position, rotation * Vector3.right * 0.1f);
    }

    private Matrix4x4[] ToMatrixArray(MatrixContainer container)
    {
        List<Matrix4x4> result = new();
        for (int i = 0; i < container.datas.Length; i++)
        {
            var data = container.datas[i];
            result.Add(new Matrix4x4()
            {
                m20 = data.m20,
                m21 = data.m21,
                m22 = data.m22,
                m23 = data.m23,
                m30 = data.m30,
                m31 = data.m31,
                m32 = data.m32,
                m33 = data.m33,
                m00 = data.m00,
                m01 = data.m01,
                m02 = data.m02,
                m03 = data.m03,
                m10 = data.m10,
                m11 = data.m11,
                m12 = data.m12,
                m13 = data.m13,
            });
        }
        return result.ToArray();
    }

    private MatrixContainer JsonToMatrixContainer(string jsonText)
    {
        MatrixContainer result = JsonUtility.FromJson<MatrixContainer>("{\"datas\":" + jsonText + "}");
        return result;
    }

    private string GetJson(string path)
    {
        TextAsset result = Resources.Load<TextAsset>(path);
        return result.text;
    }

    [System.Serializable]
    private class MatrixData
    {
        public float m00;
        public float m01;
        public float m02;
        public float m03;
        public float m10;
        public float m11;
        public float m12;
        public float m13;
        public float m20;
        public float m21;
        public float m22;
        public float m23;
        public float m30;
        public float m31;
        public float m32;
        public float m33;
    }
    [System.Serializable]
    private class MatrixContainer
    {
        public MatrixData[] datas;
    }
}