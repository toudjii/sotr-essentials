//Anthony Ackermans
using UnityEngine;

namespace ToolExtensions
{
    public class VertexData
    {

        public Vector3 VertexNormal;
        public Vector3 WorldPosition;
        public Vector3 LocalPosition;

        public VertexData( Vector3 VertexNormal, Vector3 WorldPosition, Vector3 LocalPosition)
        {

            this.VertexNormal = VertexNormal;
            this.WorldPosition = WorldPosition;
            this.LocalPosition = LocalPosition;
        }

        public void Log()
        {
            Debug.Log($" VertexNormal: {VertexNormal} --- WorldPosition: {WorldPosition} --- Localposition: {LocalPosition}");
        }
    }
}