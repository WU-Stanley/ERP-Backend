import urllib.request
import urllib.error
import json

url = "https://localhost:8000/api/Employees"
try:
    req = urllib.request.Request(url, method='GET')
    import ssl
    ctx = ssl.create_default_context()
    ctx.check_hostname = False
    ctx.verify_mode = ssl.CERT_NONE
    
    response = urllib.request.urlopen(req, context=ctx)
    data = json.loads(response.read().decode())
    
    # Assuming data is ApiResponse<PaginatedResponse<EmployeeDirectoryDto>>
    items = data.get('data', {}).get('items', [])
    if not items:
        # maybe it's ApiResponse<IEnumerable<EmployeeDirectoryDto>> ?
        if isinstance(data.get('data'), list):
            items = data['data']

    valid_users = [emp for emp in items if emp.get('userId') and emp['userId'] != '00000000-0000-0000-0000-000000000000']
    empty_users = [emp for emp in items if not emp.get('userId') or emp['userId'] == '00000000-0000-0000-0000-000000000000']
    
    print(f"Total employees: {len(items)}")
    print(f"Employees with valid userId: {len(valid_users)}")
    print(f"Employees with empty userId: {len(empty_users)}")
    
    if valid_users:
        print("First valid user:", valid_users[0]['fullName'], valid_users[0]['userId'])
    else:
        print("No valid users found.")
except Exception as e:
    print(f"Error: {e}")
