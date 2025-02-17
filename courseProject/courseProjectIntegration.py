from sympy import expand, Symbol, diff

l1 = Symbol('l1')
l2 = Symbol('l2')
l3 = Symbol('l3')
dl1 = Symbol('d1l')
dl2 = Symbol('d2l')
dl3 = Symbol('d3l')

#функции
# psi1 = 0.5*l1*(3*l1-1)*(3*l1-2)
# psi2 = 0.5*l2*(3*l2-1)*(3*l2-2)
# psi3 = 0.5*l3*(3*l3-1)*(3*l3-2)
# psi4 = 4.5*l1*l2*(3*l1-1)
# psi5 = 4.5*l1*l2*(3*l2-1)
# psi6 = 4.5*l2*l3*(3*l2-1)
# psi7 = 4.5*l2*l3*(3*l3-1)
# psi8 = 4.5*l3*l1*(3*l3-1)
# psi9 = 4.5*l3*l1*(3*l1-1)
# psi10 = 27*l1*l2*l3

psi1 = 1.0*l1*(2*l1-1)
psi2 = 1.0*l2*(2*l2-1)
psi3 = 1.0*l3*(2*l3-1)
psi4 = 4*l1*l2
psi5 = 4*l2*l3
psi6 = 4*l1*l3

psi = [psi1, psi2, psi3, psi4, psi5, psi6]

#производные
p1 = dl1*diff(psi1, l1)+dl2*diff(psi1, l2)+dl3*diff(psi1, l3)
p2 = dl1*diff(psi2, l1)+dl2*diff(psi2, l2)+dl3*diff(psi2, l3)
p3 = dl1*diff(psi3, l1)+dl2*diff(psi3, l2)+dl3*diff(psi3, l3)
p4 = dl1*diff(psi4, l1)+dl2*diff(psi4, l2)+dl3*diff(psi4, l3)
p5 = dl1*diff(psi5, l1)+dl2*diff(psi5, l2)+dl3*diff(psi5, l3)
p6 = dl1*diff(psi6, l1)+dl2*diff(psi6, l2)+dl3*diff(psi6, l3)


p=[p1, p2, p3, p4, p5, p6]

def fac(n):
    if n == 0:
        return 1
    return fac(n-1) * n

def Integrate(func):
    res = [0, 0, 0]
    func = func.replace("- 2", "-2")
    func = func.replace("- 3", "-3")
    func = func.replace("- 4", "-4")
    func = func.replace("- 1", "-1")
    func = func.replace("- 5", "-5")
    func = func.replace("- 6", "-6")
    func = func.replace("- 7", "-7")
    func = func.replace("- 8", "-8")
    func = func.replace("- 9", "-9")
    func = func.replace('+ ', '')
    func = func.replace("**", "^")
    func = func.split()
    func = [i.split("*", 1) for i in func]
    for iter in range(len(func)):
        a = float(func[iter][0])
        f = func[iter][1].rfind('l1^')
        if f>=0:
            i = int(func[iter][1][func[iter][1].rfind('l1^')+3])
        else:
            f = func[iter][1].rfind('l1')
            if f>=0:
                i = 1
            else:
                i = 0
        f = func[iter][1].rfind('l2^')
        if f>=0:
            j = int(func[iter][1][func[iter][1].rfind('l2^')+3])
        else:
            f = func[iter][1].rfind('l2')
            if f>=0:
                j = 1
            else:
                j = 0
        f = func[iter][1].rfind('l3^')
        if f>=0:
            k = int(func[iter][1][func[iter][1].rfind('l3^')+3])
        else:
            f = func[iter][1].rfind('l3')
            if f>=0:
                k = 1
            else:
                k = 0
        
        res[0]+=a * fac(i+1)*fac(j)*fac(k)/fac(i+j+k+3)
        res[1]+=a * fac(i)*fac(j+1)*fac(k)/fac(i+j+k+3)
        res[2]+=a * fac(i)*fac(j)*fac(k+1)/fac(i+j+k+3)
        
    return res

def makeM():
    f1 = open('Mrz.txt', 'w')
    mf = []
    rs = [[], [], [], [], [], []]
    for i in range(6):
        for j in range(6):
            r = str(expand(psi[i]*psi[j]))
            mf.append(r)
            rs[i].append(Integrate(r))
    i, j = 0, 0
    for row in rs:
        for x in row:
            f1.write(str(i) +" "+ str(j) +" " + ' '.join(["{0:.20f}".format(float(elem)) for elem in x]) + "\n")
            j+=1
        i+=1
        j=0
    f1.close()
    return rs

def IntegrateGrad(func):
    res = [[0,0,0], [0,0,0], [0,0,0], [0,0,0], [0,0,0], [0,0,0]]
    func = func.replace("- 2", "-2")
    func = func.replace("- 3", "-3")
    func = func.replace("- 4", "-4")
    func = func.replace("- 1", "-1")
    func = func.replace("- 5", "-5")
    func = func.replace("- 6", "-6")
    func = func.replace("- 7", "-7")
    func = func.replace("- 8", "-8")
    func = func.replace("- 9", "-9")
    func = func.replace('+ ', '')
    func = func.replace("**", "^")
    func = func.split()
    func = [i.split("*", 1) for i in func]
    for iter in range(len(func)):
        if func[iter][1].rfind("d1l^2")>=0: flag = 0
        elif func[iter][1].rfind("d1l*d2l")>=0: flag = 1
        elif func[iter][1].rfind("d1l*d3l")>=0: flag = 2
        elif func[iter][1].rfind("d2l^2")>=0: flag = 3
        elif func[iter][1].rfind("d2l*d3l")>=0: flag = 4
        elif func[iter][1].rfind("d3l^2")>=0: flag = 5
        a = float(func[iter][0])
        f = func[iter][1].rfind('l1^')
        if f>=0:
            i = int(func[iter][1][func[iter][1].rfind('l1^')+3])
        else:
            f = func[iter][1].rfind('l1')
            if f>=0:
                i = 1
            else:
                i = 0
        f = func[iter][1].rfind('l2^')
        if f>=0:
            j = int(func[iter][1][func[iter][1].rfind('l2^')+3])
        else:
            f = func[iter][1].rfind('l2')
            if f>=0:
                j = 1
            else:
                j = 0
        f = func[iter][1].rfind('l3^')
        if f>=0:
            k = int(func[iter][1][func[iter][1].rfind('l3^')+3])
        else:
            f = func[iter][1].rfind('l3')
            if f>=0:
                k = 1
            else:
                k = 0
        res[flag][0]+=a * fac(i+1)*fac(j)*fac(k)/fac(i+j+k+3)
        res[flag][1]+=a * fac(i)*fac(j+1)*fac(k)/fac(i+j+k+3)
        res[flag][2]+=a * fac(i)*fac(j)*fac(k+1)/fac(i+j+k+3)
        
    return res

#L(x,y)=a0i+a1ix+a2iy
#rs=[dl1^2 dl1dl2 dl1dl3 dl2^2 dl2dl3 dl3dl3]
def makeG():
    f2 = open('Grz.txt', 'w')
    rs = []
    for i in range(6):
        rs.append([])
        for j in range(6):
            rs[i].append([])

    for i in range(6):
        for j in range(6):
            r = str(expand(p[i]*p[j]))
            rs[i][j]=IntegrateGrad(r)
    i=0 
    j = 0
    k = 0
    for row in rs:
        for x in row:
            for y in x:
                f2.write(str(i) +" "+ str(j) +" "+ str(k) +" "+ ' '.join(["{0:.20f}".format(float(elem)) for elem in y]) + "\n")
                k+=1
            k=0
            j+=1
        i+=1
        j=0
    f2.close()
    return rs

M = makeM()
G=makeG()
