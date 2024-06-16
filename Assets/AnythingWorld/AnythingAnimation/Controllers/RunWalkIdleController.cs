namespace AnythingWorld.Animation
{
    public class RunWalkIdleController : LegacyAnimationController
    {
        private const float TimeJump = 0.2f;
        private AnimationState currentState = AnimationState.idle;
        public float runThreshold = 0.5f;
        public float walkThreshold = 1f;

        // transit the animation between walk, run and idle
        public void BlendAnimationOnSpeed(float speed)
        {
            if (currentState == AnimationState.jump_start || currentState == AnimationState.jump_fall || currentState == AnimationState.jump_land)
            {
                return;
            }
            if (speed > runThreshold)
            {
                Run();
            }
            else if (speed > walkThreshold)
            {
                Walk();
            }
            else
            {
                Idle();
            }
        }
        // transit the animation between walk and idle
        public void BlendAnimationOnSpeed(float speed, float walkThreshold)
        {
            if (currentState == AnimationState.jump_start || currentState == AnimationState.jump_fall || currentState == AnimationState.jump_land)
            {
                return;
            }
            if (speed > walkThreshold)
            {
                Walk();
            }
            else
            {
                Idle();
            }
        }
        //call the walk animation and change the state
        public void Walk()
        {
            if (currentState != AnimationState.walk)
            {
                base.CrossFadeAnimation("walk");
                currentState = AnimationState.walk;
            }

        }
        //call the run animation and change the state
        public void Run()
        {
            if (currentState != AnimationState.run)
            {
                base.CrossFadeAnimation("run");
                currentState = AnimationState.run;
            }
        }
        //call the idle animation and change the state
        public void Idle()
        {
            if (currentState != AnimationState.idle)
            {
                base.CrossFadeAnimation("idle");
                currentState = AnimationState.idle;
            }
        }
        //call the jump animation and change the state
        public void JumpStart()
        {
            if (currentState != AnimationState.jump_start)
            {
                base.CrossFadeAnimation("jump_start");
                currentState = AnimationState.jump_start;
                Invoke("ExitJump", TimeJump);
            }
        }
        //call the end of jump_start and start the jump_fall
        private void ExitJump()
        {
            if (currentState == AnimationState.jump_start)
            {
                JumpFall();
            }
        }
        //call the jump_fall animation and change the state
        private void JumpFall()
        {
            if (currentState != AnimationState.jump_fall)
            {
                base.CrossFadeAnimation("jump_fall");
                currentState = AnimationState.jump_fall;
            }
        }
        //Keep calling the jump_fall animation until the character land
        public void JumpFall(bool fall)
        {
            if(currentState == AnimationState.jump_start)
            {
                return;
            }
            if (currentState != AnimationState.jump_fall && fall)
            {
                base.CrossFadeAnimation("jump_fall");
                currentState = AnimationState.jump_fall;
            }
            else if (!fall)
            {
                if(currentState == AnimationState.jump_fall)
                {
                    Land();
                }
            }
        }
        //call the jump_end animation and change the state
        public void Land()
        {
            if (currentState != AnimationState.jump_land)
            {
                base.CrossFadeAnimation("jump_end");
                currentState = AnimationState.jump_land;
                Invoke("ExitLand", 0.2f);
            }
        }
        //call the end of jump_end and start the idle
        private void ExitLand()
        {
            if (currentState == AnimationState.jump_land)
            {
                Idle();
            }
        }
        
        private enum AnimationState
        {
            idle,
            walk,
            run,
            jump_start,
            jump_fall,
            jump_land
        }

    }
}
