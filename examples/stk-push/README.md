# STK Push (M-Pesa) Integration Example

## Overview
This example demonstrates how to integrate the Salama Security Protocol with M-Pesa STK Push for mobile money transactions.

## Features
- ✅ Reversed PIN detection via USSD
- ✅ Transaction quarantine in shadow mode
- ✅ M-Pesa callback handling

## Integration Flow

```
1. User initiates payment via M-Pesa
2. M-Pesa prompts for PIN
3. User enters PIN (normal or reversed)
4. M-Pesa sends callback to Wekeza API
5. Salama Protocol checks for reversed PIN
6. If reversed → Shadow mode (quarantine)
7. If normal → Standard mode (execute)
```

## Example Implementation

```csharp
[HttpPost("api/stk/callback")]
public async Task<IActionResult> HandleStkCallback([FromBody] MpesaCallback callback)
{
    var stkAdapter = new StkPushChannelAdapter();
    
    // Create auth request from M-Pesa callback
    var authRequest = stkAdapter.CreateAuthRequest(
        phoneNumber: callback.PhoneNumber,
        pinCode: callback.PIN,
        transactionRef: callback.TransactionRef
    );
    
    // Authenticate (checks for reversed PIN)
    var authResponse = await _authService.AuthenticateAsync(authRequest);
    
    if (authResponse.SessionMode == SessionMode.Shadow)
    {
        // Quarantine transaction
        var response = stkAdapter.FormatShadowModeResponse(
            amount: callback.Amount,
            recipient: callback.Recipient
        );
        
        return Ok(new { success = true, message = response });
    }
    else
    {
        // Execute real transaction
        var result = await _bankingService.ProcessTransferAsync(...);
        return Ok(result);
    }
}
```

## USSD Code Examples

### Normal PIN Entry
```
User dials: *234*1234#
Result: Standard mode, transaction executes
```

### Reversed PIN Entry (Duress)
```
User dials: *234*4321#
Result: Shadow mode, transaction quarantined
```

## Limitations
- No accelerometer data (relies on reversed PIN only)
- Depends on M-Pesa USSD code format
- Limited behavioral signals

## See Also
- [Multi-Channel Guide](../../docs/MULTI_CHANNEL_GUIDE.md)
- [API Reference](../../docs/API_REFERENCE.md)
