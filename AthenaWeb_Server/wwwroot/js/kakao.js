window.kakaoInit = () => {
    Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
}

window.kakaoLogin = () => {

    Kakao.Auth.login({
        success: function (response) {
            Kakao.API.request({
                url: '/v2/user/me',
                success: function (response) {
                    console.log(response.id, response.kakao_account.profile.nickname)
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

    let isLogin = Kakao.Auth.getAccessToken() ? true : false;
    return isLogin;
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
            },
        })
        Kakao.Auth.setAccessToken(undefined)
    }

    let isLogin = Kakao.Auth.getAccessToken() ? true : false;
    return isLogin;
}

