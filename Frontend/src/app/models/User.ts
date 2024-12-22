export interface UserRegisterValues {
    region: string;
    username: string;
    password: string;
    avatar: string;
    email: string;
}

export interface UserLoginValues {
    region: string;
    username: string;
    password: string;
}

export interface User {
    region: string;
    username: string;
    avatar: string;
}