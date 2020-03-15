using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;


///Sent form server to client.
public enum ServerPackets
{
    welcome = 1,
    spawnPlayer,
    playerPosition,
    playerRotation,
    playerDisonnected,
    playerHealth,
    playerRespawned
}

///Sent form client to server
public enum ClientPackets
{
    welcomeReceived = 1,
    playerMovement,
    playerShoot

}

public class Packet : IDisposable
{
    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos;

    ///Creates a new empty packet (without an ID)
    public Packet()
    {
        buffer = new List<byte>(); //initialize buffer
        readPos = 0; //set readPos to 0
    }

    ///Creates a new packet with a given ID. Used for sending
    ///<param name="__id">The packet ID</param>
    public Packet(int __id)
    {
        buffer = new List<byte>(); //initialize buffer
        readPos = 0; //set readPos to 0


        Write(__id); // write the packet id to the buffer
    }

    ///Creates a packet from which data can be read. Used for receiving
    ///<param name="__data">
    public Packet(byte[] __data)
    {
        buffer = new List<byte>(); //initialize buffer
        readPos = 0;

        SetBytes(__data);
    }

    #region Functions
    ///Sets the packet's content and rpepares it to be read
    ///<param name="__data">the bytes to add to the packet
    public void SetBytes(byte[] __data)
    {
        Write(__data);
        readableBuffer = buffer.ToArray();
    }

    ///Inserts the length of the packet's content at teh start of the buffer
    public void Writelength()
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); //Insert the byte length of the packet at the very beginning
    }

    ///Inserts the given int at the start of the buffer
    public void InsertInt(int __value)
    {
        buffer.InsertRange(0, BitConverter.GetBytes(__value));
    }

    ///Gets the packet's content in an array form
    public byte[] ToArray()
    {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }

    ///Gets the length of the packet's content
    public int Length()
    {
        return buffer.Count;
    }

    ///Gets the length of the unread data contained in teh packet 
    public int UnreadLength()
    {
        return Length() - readPos;
    }

    ///Resets the packet instance to allow it ot be reused.
    ///<param name="__shouldReset">whether or no to reset the packet
    public void Reset(bool __shouldReset = true)
    {
        if (__shouldReset)
        {
            buffer.Clear();
            readableBuffer = null;
            readPos = 0;
        }
        else
        {
            readPos -= 4; // unread the last read int (4 - bytes)
        }
    }
    #endregion

    #region Write Data
    ///Adds an array of bytes to the packet
    ///<param name="__value">the byte to add<\param>
    public void write(byte __value)
    {
        buffer.Add(__value);
    }

    ///Adds an array of bytes to the packet
    ///<param name="__value">the byte array to add
    public void Write(byte[] __value)
    {
        buffer.AddRange(__value);
    }

    ///Adds a short to the packet
    ///<param name="__value">the short to add
    public void Write(short __value)
    {
        buffer.AddRange(BitConverter.GetBytes(__value));
    }

    /// <summary>Adds an int to the packet.</summary>
    /// <param name="_value">The int to add.</param>
    public void Write(int _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    /// <summary>Adds a long to the packet.</summary>
    /// <param name="_value">The long to add.</param>
    public void Write(long _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    /// <summary>Adds a float to the packet.</summary>
    /// <param name="_value">The float to add.</param>
    public void Write(float _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    /// <summary>Adds a bool to the packet.</summary>
    /// <param name="_value">The bool to add.</param>
    public void Write(bool _value)
    {
        buffer.AddRange(BitConverter.GetBytes(_value));
    }

    /// <summary>Adds a string to the packet.</summary>
    /// <param name="_value">The string to add.</param>
    public void Write(string _value)
    {
        Write(_value.Length); // Add the length of the string to the packet
        buffer.AddRange(Encoding.ASCII.GetBytes(_value)); // Add the string itself
    }

    /// <summary>Adds a Vector3 to the packet.</summary>
    /// <param name="_value">The Vector3 to add.</param>
    public void Write(Vector3 _value)
    {
        Write(_value.x);
        Write(_value.y);
        Write(_value.z);
    }

    /// <summary>Adds a Quaternion to the packet.</summary>
    /// <param name="_value">The quaternion to add.</param>
    public void Write(Quaternion _value)
    {
        Write(_value.x);
        Write(_value.y);
        Write(_value.z);
        Write(_value.w);
    }
    #endregion

    #region Read Data
    ///Reads a byte from the packet
    ///<param name="__moveReadPos">whete or not to move the buffer's readPos
    public byte ReadByte(bool __moveReadPos = true)
    {
        if (buffer.Count > readPos) //if there are unread bytes
        {
            byte __value = readableBuffer[readPos]; // get the byte at the readPos (position)
            if (__moveReadPos) // if __moveReadPos is true
            {
                readPos += 1;
            }
            return __value; //return byte
        }
        else
        {
            throw new Exception("Could not read value ot type 'byte'.");
        }
    }

    /// <param name="_length">The length of the byte array.</param>
    /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
    public byte[] ReadBytes(int __length, bool __moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            byte[] __value = buffer.GetRange(readPos, __length).ToArray();
            if (__moveReadPos)
            {
                readPos += __length;
            }
            return __value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte[]'.");
        }
    }

    /// <summary>Reads a short from the packet.</summary>
    /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
    public short ReadShort(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            short __value = BitConverter.ToInt16(readableBuffer, readPos); // Convert the bytes to a short
            if (_moveReadPos)
            {
                // If _moveReadPos is true and there are unread bytes
                readPos += 2; // Increase readPos by 2
            }
            return __value; // Return the short
        }
        else
        {
            throw new Exception("Could not read value of type 'short'!");
        }
    }

    /// <summary>Reads an int from the packet.</summary>
    /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
    public int ReadInt(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            int __value = BitConverter.ToInt32(readableBuffer, readPos); // Convert the bytes to an int
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 4; // Increase readPos by 4
            }
            return __value; // Return the int
        }
        else
        {
            throw new Exception("Could not read value of type 'int'!");
        }
    }

    /// <summary>Reads a long from the packet.</summary>
    /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
    public long ReadLong(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            long __value = BitConverter.ToInt64(readableBuffer, readPos); // Convert the bytes to a long
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 8; // Increase readPos by 8
            }
            return __value; // Return the long
        }
        else
        {
            throw new Exception("Could not read value of type 'long'!");
        }
    }

    /// <summary>Reads a float from the packet.</summary>
    /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
    public float ReadFloat(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            float __value = BitConverter.ToSingle(readableBuffer, readPos); // Convert the bytes to a float
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 4; // Increase readPos by 4
            }
            return __value; // Return the float
        }
        else
        {
            throw new Exception("Could not read value of type 'float'!");
        }
    }

    /// <summary>Reads a bool from the packet.</summary>
    /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
    public bool ReadBool(bool _moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            // If there are unread bytes
            bool __value = BitConverter.ToBoolean(readableBuffer, readPos); // Convert the bytes to a bool
            if (_moveReadPos)
            {
                // If _moveReadPos is true
                readPos += 1; // Increase readPos by 1
            }
            return __value; // Return the bool
        }
        else
        {
            throw new Exception("Could not read value of type 'bool'!");
        }
    }

    /// <summary>Reads a string from the packet.</summary>
    /// <param name="_moveReadPos">Whether or not to move the buffer's read position.</param>
    public string ReadString(bool _moveReadPos = true)
    {
        try
        {
            int _length = ReadInt(); // Get the length of the string
            string __value = Encoding.ASCII.GetString(readableBuffer, readPos, _length); // Convert the bytes to a string
            if (_moveReadPos && __value.Length > 0)
            {
                // If _moveReadPos is true string is not empty
                readPos += _length; // Increase readPos by the length of the string
            }
            return __value; // Return the string
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    public Vector3 ReadVector3(bool _moveReadPos = true)
    {
        return new Vector3(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
    }

    public Quaternion ReadQuaternion(bool _moveReadPos = true)
    {
        return new Quaternion(ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos), ReadFloat(_moveReadPos));
    }
    #endregion

    private bool disposed = false;

    protected virtual void Dispose(bool __disposing)
    {
        if (!disposed)
        {
            if (__disposing)
            {
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
