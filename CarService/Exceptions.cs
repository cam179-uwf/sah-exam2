namespace CarController.Services;

/**
 * The classes in this file are just for c# exceptions.
 */

public class BluetoothNotAvailableException(string message) : Exception(message);
public class BluetoothDeviceNotFoundException(string message) : Exception(message);
public class BluetoothDeviceFailedToConnectException(string message) : Exception(message);
public class CarServiceException(string message) : Exception(message);