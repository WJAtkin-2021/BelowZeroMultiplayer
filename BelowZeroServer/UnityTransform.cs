namespace BelowZeroServer
{
    public class UnityTransform
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public UnityTransform()
        {
            position = new Vector3();
            rotation = new Quaternion();
            scale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public UnityTransform(Vector3 _position)
        {
            position = _position;
            rotation = new Quaternion();
            scale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public UnityTransform(Vector3 _position, Quaternion _rotation)
        {
            position = _position;
            rotation = _rotation;
            scale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        public UnityTransform(Vector3 _position, Quaternion _rotation, Vector3 _scale)
        {
            position = _position;
            rotation = _rotation;
            scale = _scale;
        }
    }
}
