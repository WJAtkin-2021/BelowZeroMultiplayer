using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BelowZeroClient
{
    public class Packet : IDisposable
    {
        private List<byte> m_buffer;
        private byte[] m_readableBuffer;
        private int m_readPos;

        #region Constructors

        /// <summary>
        /// Creates a new empty packet
        /// </summary>
        public Packet()
        {
            m_buffer = new List<byte>();
            m_readPos = 0;
        }

        /// <summary>
        /// Creates a new packet with a given ID. Used for sending data.
        /// </summary>
        /// <param name="_id"></param>
        public Packet(int _id)
        {
            m_buffer = new List<byte>();
            m_readPos = 0;

            Write(_id);
        }

        /// <summary>
        /// Creates a packets who's data can be read. Used for receiving data.
        /// </summary>
        /// <param name="_data"></param>
        public Packet(byte[] _data)
        {
            m_buffer = new List<byte>();
            m_readPos = 0;

            SetBytes(_data);
        }

        #endregion

        #region Helper Methods

        public void SetBytes(byte[] _data)
        {
            Write(_data);
            m_readableBuffer = m_buffer.ToArray();
        }

        public void WriteLength()
        {
            m_buffer.InsertRange(0, BitConverter.GetBytes(m_buffer.Count));
        }

        public void InsertInt(int _value)
        {
            m_buffer.InsertRange(0, BitConverter.GetBytes(_value));
        }

        public void InsertUint(uint _value)
        {
            m_buffer.InsertRange(0, BitConverter.GetBytes(_value));
        }

        public byte[] ToArray()
        {
            m_readableBuffer = m_buffer.ToArray();
            return m_readableBuffer;
        }

        public int Length()
        {
            return m_buffer.Count;
        }

        public int UnreadLength()
        {
            return Length() - m_readPos;
        }

        /// <summary>
        /// Allows the packet to be reused by another handler
        /// </summary>
        /// <param name="_shouldReset"></param>
        public void Reset(bool _shouldReset = true)
        {
            if (_shouldReset)
            {
                m_buffer.Clear();
                m_readableBuffer = null;
                m_readPos = 0;
            }
            else
            {
                m_readPos -= 4; // Unread the last int
            }
        }

        #endregion

        #region Write Methods

        /// <summary>
        /// Writes a single byte to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(byte _value)
        {
            m_buffer.Add(_value);
        }

        /// <summary>
        /// Writes an array of bytes to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(byte[] _value)
        {
            m_buffer.AddRange(_value);
        }

        /// <summary>
        /// Writes a short to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(short _value)
        {
            m_buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Writes an int to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(int _value)
        {
            m_buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Writes an uint to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(uint _value)
        {
            m_buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Writes a long to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(long _value)
        {
            m_buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Writes a float to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(float _value)
        {
            m_buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Writes a bool to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(bool _value)
        {
            m_buffer.AddRange(BitConverter.GetBytes(_value));
        }

        /// <summary>
        /// Writes a string to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(string _value)
        {
            Write(_value.Length);
            m_buffer.AddRange(Encoding.ASCII.GetBytes(_value));
        }

        /// <summary>
        /// Writes a Vector2 to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(Vector2 _value)
        {
            Write(_value.x);
            Write(_value.y);
        }

        /// <summary>
        /// Writes a Vector3 to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(Vector3 _value)
        {
            Write(_value.x);
            Write(_value.y);
            Write(_value.z);
        }

        /// <summary>
        /// Writes a Quaternion to the packet
        /// </summary>
        /// <param name="_value"></param>
        public void Write(Quaternion _value)
        {
            Write(_value.x);
            Write(_value.y);
            Write(_value.z);
            Write(_value.w);
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Reads a byte from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public byte ReadByte(bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                byte value = m_readableBuffer[m_readPos];
                if (_moveReadPos)
                {
                    m_readPos += 1;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'byte'!");
                throw new Exception("Could not read value of type 'byte'!");
            }
        }

        /// <summary>
        /// Reads a byte array from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public byte[] ReadBytes(int _length, bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                byte[] value = m_buffer.GetRange(m_readPos, _length).ToArray();
                if (_moveReadPos)
                {
                    m_readPos += _length;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'byte[]'!");
                throw new Exception("Could not read value of type 'byte[]'!");
            }
        }

        /// <summary>
        /// Reads a short from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public short ReadShort(bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                short value = BitConverter.ToInt16(m_readableBuffer, m_readPos);
                if (_moveReadPos)
                {
                    m_readPos += 2;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'short'!");
                throw new Exception("Could not read value of type 'short'!");
            }
        }

        /// <summary>
        /// Reads a int from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public int ReadInt(bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                int value = BitConverter.ToInt32(m_readableBuffer, m_readPos);
                if (_moveReadPos)
                {
                    m_readPos += 4;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'int'!");
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>
        /// Reads a uint from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public uint ReadUint(bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                uint value = BitConverter.ToUInt32(m_readableBuffer, m_readPos);
                if (_moveReadPos)
                {
                    m_readPos += 4;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'int'!");
                throw new Exception("Could not read value of type 'int'!");
            }
        }

        /// <summary>
        /// Reads a long from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public long ReadLong(bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                long value = BitConverter.ToInt64(m_readableBuffer, m_readPos);
                if (_moveReadPos)
                {
                    m_readPos += 8;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'long'!");
                throw new Exception("Could not read value of type 'long'!");
            }
        }

        /// <summary>
        /// Reads a float from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public float ReadFloat(bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                float value = BitConverter.ToSingle(m_readableBuffer, m_readPos);
                if (_moveReadPos)
                {
                    m_readPos += 4;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'float'!");
                throw new Exception("Could not read value of type 'float'!");
            }
        }

        /// <summary>
        /// Reads a bool from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public bool ReadBool(bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                bool value = BitConverter.ToBoolean(m_readableBuffer, m_readPos);
                if (_moveReadPos)
                {
                    m_readPos += 1;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'bool'!");
                throw new Exception("Could not read value of type 'bool'!");
            }
        }

        /// <summary>
        /// Reads a string from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public string ReadString(bool _moveReadPos = true)
        {
            if (m_buffer.Count > m_readPos)
            {
                int length = ReadInt();
                string value = Encoding.ASCII.GetString(m_readableBuffer, m_readPos, length);
                if (_moveReadPos && value.Length > 0)
                {
                    m_readPos += length;
                }

                return value;
            }
            else
            {
                FileLog.Log("Could not read value of type 'String'!");
                throw new Exception("Could not read value of type 'String'!");
            }
        }

        /// <summary>
        /// Reads a Vector2 from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public Vector2 ReadVector2(bool _moveReadPos = true)
        {
            return new Vector2(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }

        /// <summary>
        /// Reads a Vector3 from a packet
        /// </summary>
        /// <param name="_moveReadPos">If true moves the reading position along the packet, can be false to peek the next variable.</param>
        /// <returns></returns>
        public Vector3 ReadVector3(bool _moveReadPos = true)
        {
            return new Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }

        /// <summary>
        /// Reads a Quaternion from a packet
        /// </summary>
        /// <param name="_moveReadPos"></param>
        /// <returns></returns>
        public Quaternion ReadQuaternoin(bool _moveReadPos = true)
        {
            return new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
        }

        #endregion

        #region IDisposable Implementation

        private bool m_disposedValue;

        protected virtual void Dispose(bool _disposing)
        {
            if (!m_disposedValue)
            {
                if (_disposing)
                {
                    // Dispose managed state (managed objects)
                    m_buffer = null;
                    m_readableBuffer = null;
                    m_readPos = 0;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                m_disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(_disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
