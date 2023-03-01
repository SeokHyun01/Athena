window.kakaoLogin = async () => {
    try {
        //카카오 초기화
        if (!Kakao.isInitialized()) {
            Kakao.init('8743aa8ae98410121d5eb950d8d9fe56');
        }

        //카카오 로그인
        await new Promise((resolve, reject) => {
            Kakao.Auth.login({
                success: function (response) {
                    resolve();
                },
                fail: function (error) {
                    reject(error);
                },
            })
        });

        //카카오 로그인 정보 가져오기
        let response = await new Promise((resolve, reject) => {
            Kakao.API.request({
                url: '/v2/user/me',
                success: function (response) {
                    resolve(response);
                },
                fail: function (error) {
                    reject(error);
                },
            })
        });

        //카카오 로그인 정보 출력
        console.log(response.id, response.kakao_account.profile.nickname);
        return true;
    } catch (error) {
        console.log(error);
        return false;
    }
}


window.kakaoLogout = async () => {
    try {
        if (Kakao.Auth.getAccessToken()) {
            //카카오 로그아웃
            await new Promise((resolve, reject) => {
                Kakao.API.request({
                    url: '/v1/user/unlink',
                    success: function (response) {
                        resolve(response);
                    },
                    fail: function (error) {
                        reject(error);
                    },
                })
            });
            //카카오 토큰 삭제
            // Kakao.Auth.setAccessToken(undefined);
        }

        return false;
    } catch (error) {
        console.log(error);
        return true;
    }
}
