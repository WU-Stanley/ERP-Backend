import urllib.request
import urllib.error

url = "https://localhost:8000/api/auth/impersonate/00000000-0000-0000-0000-000000000000"
try:
    req = urllib.request.Request(url, method='OPTIONS')
    req.add_header('Origin', 'http://localhost:4200')
    req.add_header('Access-Control-Request-Method', 'POST')
    import ssl
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    
    response = urllib.request.urlopen(req, context=ctx)
    print("Status:", response.status)
    print(response.headers)
except urllib.error.HTTPError as e:
    print(f"HTTP Error: {e.code}")
    print(e.read().decode())
except Exception as e:
    print(f"Error: {e}")
