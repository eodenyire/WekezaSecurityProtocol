# Web Portal Integration Example

## Overview
This example demonstrates how to integrate the Salama Security Protocol into a web banking portal.

## Features
- ✅ Mouse tremor detection
- ✅ Keyboard pattern analysis
- ✅ Browser fingerprinting
- ✅ Reversed PIN detection

## Implementation

```html
<!DOCTYPE html>
<html>
<head>
    <title>Wekeza Bank - Login</title>
</head>
<body>
    <form id="loginForm">
        <input type="text" id="accountId" placeholder="Account ID" />
        <input type="password" id="pin" placeholder="PIN" maxlength="6" />
        <button type="submit">Login</button>
    </form>

    <script src="salama-web.js"></script>
    <script>
        const salamaAuth = new WekeziWebAuth();
        
        document.getElementById('loginForm').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            salamaAuth.startPINEntry();
            
            const accountId = document.getElementById('accountId').value;
            const pin = document.getElementById('pin').value;
            
            await salamaAuth.login(accountId, pin);
        });
        
        // Track behavioral data
        document.getElementById('pin').addEventListener('focus', () => {
            salamaAuth.startPINEntry();
        });
    </script>
</body>
</html>
```

## JavaScript Implementation

See `salama-web.js` for complete implementation including:
- Mouse movement tracking
- Typing pattern analysis
- Browser fingerprinting
- Tab switch detection

## Security Features
- Client-side PIN hashing (SHA-256)
- Secure session storage
- Certificate pinning
- HTTPS only

## See Also
- [Multi-Channel Guide](../../docs/MULTI_CHANNEL_GUIDE.md)
- [Security Guidelines](../../docs/SECURITY.md)
