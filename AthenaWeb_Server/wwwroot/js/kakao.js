window.kakaoLogin = () => {
    Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
    console.log(Kakao.isInitialized());

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

    if(Kakao.Auth.getAccessToken()) {
        console.log(Kakao.Auth.getAccessToken())
        return true;
    }else{
        return false;
    }
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

