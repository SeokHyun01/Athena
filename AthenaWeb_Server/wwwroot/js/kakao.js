window.kakaoLogin = () => {
    Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
    console.log(Kakao.isInitialized());

    Kakao.Auth.login({
        success: function (response) {
            Kakao.API.request({
                url: '/v2/user/me',
                success: function (response) {
                    console.log(response)
                    console.log(response.id, response.nickname)
                    return true;    
                },
                fail: function (error) {
                    console.log(error)
                },
            })
        },
        fail: function (error) {
            console.log(error)
        },
    })
    return false;
}

window.kakaoLogout = () => {
    if (Kakao.Auth.getAccessToken()) {
        Kakao.API.request({
            url: '/v1/user/unlink',
            success: function (response) {
                console.log(response)
            },
            fail: function (error) {
                console.log(error)
                return true;
            },
        })
        Kakao.Auth.setAccessToken(undefined)
    }
    return false;
}

