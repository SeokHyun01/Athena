window.kakaoLogin = () => {
    Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
    console.log(Kakao.isInitialized());
    let isLogin = false;

    Kakao.Auth.login({
        success: function (response) {
            Kakao.API.request({
                url: '/v2/user/me',
                success: function (response) {
                    console.log(response.id, response.kakao_account.profile.nickname)
                    isLogin = true;
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
    console.log("isLogin: " + isLogin)
    return isLogin;
}

window.kakaoLogout = () => {
    let isLogout = false;

    if (Kakao.Auth.getAccessToken()) {
        Kakao.API.request({
            url: '/v1/user/unlink',
            success: function (response) {
                console.log(response)
                isLogout = true;
            },
            fail: function (error) {
                console.log(error)
            },
        })
        Kakao.Auth.setAccessToken(undefined)
    }
    return isLogout;
}

