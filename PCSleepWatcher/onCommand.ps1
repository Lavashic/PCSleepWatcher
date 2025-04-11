# PowerShell client to send a command to the named pipe server
$pipe = New-Object System.IO.Pipes.NamedPipeClientStream(".", "PCSleepWatcherPipe", [System.IO.Pipes.PipeDirection]::Out)
$pipe.Connect()

# Send the command to the server
$writer = New-Object System.IO.StreamWriter($pipe)
$writer.WriteLine("on")
$writer.Flush()

# Close the pipe
$writer.Close()
$pipe.Close()