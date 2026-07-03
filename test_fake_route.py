import urllib.request
import urllib.error

url = "https://localhost:8000/api/auth/this_route_does_not_exist"
try:
    req = urllib.request.Request(url, method='POST')
    import ssl
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    
    response = urllib.request.urlopen(req, context=ctx)
    print("Status:", response.status)
except urllib.error.HTTPError as e:
    print(f"HTTP Error: {e.code}")
except Exception as e:
    print(f"Error: {e}")
