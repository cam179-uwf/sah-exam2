namespace CarController.Services;

public class BluetoothNotAvailableException(string message) : Exception(message);
public class BluetoothDeviceNotFoundException(string message) : Exception(message);
public class BluetoothDeviceFailedToConnectException(string message) : Exception(message);
public class CarServiceException(string message) : Exception(message);