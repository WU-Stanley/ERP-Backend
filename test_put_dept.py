import urllib.request
import urllib.error
import json

# Login to get token
login_url = "https://localhost:8000/api/auth/login"
login_data = json.dumps({"email": "superadmin@wigweuniversity.edu.ng", "password": "Password123!"}).encode('utf-8')

import ssl
ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

req = urllib.request.Request(login_url, data=login_data, headers={'Content-Type': 'application/json'}, method='POST')
response = urllib.request.urlopen(req, context=ctx)
res_data = json.loads(response.read().decode())
token = res_data['data']['token']

# Call PUT department
dept_id = "e6ddb5af-19c4-487e-81b3-08ded107c540"
put_url = f"https://localhost:8000/api/department/{dept_id}"
put_data = json.dumps({
    "id": dept_id,
    "name": "Test Dept",
    "description": "Test Desc",
    "headOfDepartmentId": "78509e56-106d-4f47-aaea-08ded113e514"
}).encode('utf-8')

req2 = urllib.request.Request(put_url, data=put_data, method='PUT')
req2.add_header('Authorization', f'Bearer {token}')
req2.add_header('Content-Type', 'application/json')

try:
    response2 = urllib.request.urlopen(req2, context=ctx)
    print("Status:", response2.status)
    print(response2.read().decode())
except urllib.error.HTTPError as e:
    print(f"HTTP Error: {e.code}")
    print(e.read().decode())
