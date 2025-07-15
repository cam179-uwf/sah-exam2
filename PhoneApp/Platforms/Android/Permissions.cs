namespace PhoneApp;

public class BluetoothConnectPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        [ ("android.permission.BLUETOOTH_CONNECT", true) ];
}

public class BluetoothScanPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        [ ("android.permission.BLUETOOTH_SCAN", true) ];
}