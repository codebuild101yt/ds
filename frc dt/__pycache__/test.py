import socket, struct, cv2, numpy as np

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.bind(("127.0.0.1", 8080))
sock.listen(1)
conn, addr = sock.accept()
print(f"Connected: {addr}")
buffer = b""

while True:
    while len(buffer) < 4:
        buffer += conn.recv(4096)
    frame_len = struct.unpack(">I", buffer[:4])[0]
    buffer = buffer[4:]

    while len(buffer) < frame_len:
        buffer += conn.recv(4096)
    frame_data = buffer[:frame_len]
    buffer = buffer[frame_len:]

    frame = cv2.imdecode(np.frombuffer(frame_data, np.uint8), cv2.IMREAD_COLOR)
    if frame is not None:
        cv2.imshow("Test", frame)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
    else:
        print(f"Failed to decode frame of length {len(frame_data)}")
